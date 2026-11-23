using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using static Makarov.FlowchartBuilder.Tools.EntityRelationship.SandboxNative;

namespace Makarov.FlowchartBuilder.Tools.EntityRelationship;

internal sealed record EntryColumn(string Name, string Type, bool Required, bool AutoId, string? ParentTable, string? ParentColumn);
internal sealed record EntryValue(string? Text);
internal sealed record EntryReference(string Key, string? Label);

internal sealed class EntryInputException(EntryColumn column, string message) : ArgumentException(message)
{
    public EntryColumn Column { get; } = column;
}

internal sealed partial class SandboxDatabase
{
    private readonly Dictionary<string, IReadOnlyList<EntryColumn>> _entryColumns = new(StringComparer.Ordinal);
    private static readonly IntPtr Transient = new(-1); // SQLite copies bound data during the bind call.

    private string FindUserTable(string name)
    {
        ObjectDisposedException.ThrowIf(_database == IntPtr.Zero, this);
        return _tables.TryGetValue(SchemaFields.NameKey(name), out var table)
            ? table : throw new ArgumentException($"Unknown table '{name}'.");
    }

    internal IReadOnlyList<EntryColumn> GetEntryColumns(string table)
    {
        table = FindUserTable(table);
        if (_entryColumns.TryGetValue(table, out var cached)) return cached;

        var fields = new List<(string Name, string Type, bool Required, bool Primary)>();
        string quoted = SchemaSql.Quote(table);
        ExecuteCore($"PRAGMA table_info({quoted})", stmt => fields.Add((
            Cell(stmt, 1, false)!, Cell(stmt, 2, false)!.ToUpperInvariant(),
            Cell(stmt, 3, false) == "1", Cell(stmt, 5, false) != "0")));

        var references = new Dictionary<string, (string? Table, string? Column)>(StringComparer.Ordinal);
        ExecuteCore($"PRAGMA foreign_key_list({quoted})", stmt =>
        {
            string child = Cell(stmt, 3, false)!;
            references[SchemaFields.NameKey(child)] = (Cell(stmt, 2, false), Cell(stmt, 4, false));
        });

        bool singleKey = fields.Count(f => f.Primary) == 1;
        var columns = fields.Select(f =>
        {
            references.TryGetValue(SchemaFields.NameKey(f.Name), out var parent);
            return new EntryColumn(f.Name, f.Type, f.Required || f.Primary, singleKey && f.Primary && f.Type == "INTEGER",
                parent.Table, parent.Column);
        }).ToArray();
        return _entryColumns[table] = Array.AsReadOnly(columns);
    }

    internal IReadOnlyList<EntryReference> GetReferenceValues(EntryColumn column)
    {
        if (column.ParentTable == null || column.ParentColumn == null) return [];
        string parent = FindUserTable(column.ParentTable);
        var fields = GetEntryColumns(parent);
        var field = fields.Single(f => SchemaFields.SameName(f.Name, column.ParentColumn));
        var label = fields.FirstOrDefault(f => f != field && f.Type == "TEXT" && SchemaFields.SameName(f.Name, "name"))
            ?? fields.FirstOrDefault(f => f != field && f.Type == "TEXT" && SchemaFields.SameName(f.Name, "title"));
        string key = "p." + SchemaSql.Quote(field.Name);
        // Only offer keys that can be entered using this column's declared type.
        string storageTypes = column.Type switch
        {
            "INTEGER" => "'integer'",
            "REAL" or "NUMERIC" => "'integer','real'",
            "TEXT" => "'text'",
            "BLOB" => "'blob'",
            _ => throw new ArgumentException($"Unsupported column type '{column.Type}'.")
        };
        var keys = new List<EntryReference>();
        string projection = label == null ? key : $"{key}, p.{SchemaSql.Quote(label.Name)}";
        ExecuteCore($"SELECT {projection} FROM {SchemaSql.Quote(parent)} AS p WHERE typeof({key}) IN ({storageTypes}) ORDER BY {key} LIMIT 100", stmt =>
        {
            string? value = ReadReferenceKey(stmt);
            if (value == null) return;
            string? caption = label == null ? null : Cell(stmt, 1, truncate: true)?.Trim();
            if (caption is { Length: > 64 })
            {
                int take = char.IsHighSurrogate(caption[62]) ? 62 : 63;
                caption = caption[..take] + "…";
            }
            keys.Add(new(value, string.IsNullOrEmpty(caption) ? null : caption));
        });
        return keys;
    }

    private static string? ReadReferenceKey(IntPtr statement)
    {
        switch (sqlite3_column_type(statement, 0))
        {
            case 2: // REAL keys must round-trip to the same binary64 value.
                double number = sqlite3_column_double(statement, 0);
                return double.IsFinite(number) ? number.ToString("R", CultureInfo.InvariantCulture) : null;
            case 4:
                var data = sqlite3_column_blob(statement, 0);
                var bytes = new byte[sqlite3_column_bytes(statement, 0)];
                if (bytes.Length > 0) Marshal.Copy(data, bytes, 0, bytes.Length);
                return Convert.ToHexString(bytes);
            default:
                return Cell(statement, 0, truncate: false);
        }
    }

    internal long InsertRow(string table, IReadOnlyDictionary<string, EntryValue> values)
    {
        table = FindUserTable(table);
        var columns = GetEntryColumns(table).ToDictionary(f => SchemaFields.NameKey(f.Name), StringComparer.Ordinal);
        var supplied = new Dictionary<EntryColumn, EntryValue>();
        foreach (var (name, value) in values)
        {
            if (!columns.TryGetValue(SchemaFields.NameKey(name), out var field) || !supplied.TryAdd(field, value))
                throw new ArgumentException($"Unknown or repeated column '{name}'.");
        }

        string insert = $"INSERT INTO {SchemaSql.Quote(table)} ";
        if (supplied.Count == 0) ExecuteCore(insert + "DEFAULT VALUES");
        else
        {
            string names = string.Join(", ", supplied.Keys.Select(f => SchemaSql.Quote(f.Name)));
            string parameters = string.Join(", ", Enumerable.Repeat("?", supplied.Count));
            ExecuteCore($"{insert}({names}) VALUES ({parameters})", bind: stmt =>
            {
                int index = 1;
                foreach (var (field, value) in supplied) BindEntry(stmt, index++, field, value.Text);
            });
        }
        return sqlite3_last_insert_rowid(_database);
    }

    internal SandboxResult PreviewInsertedRow(string table, long rowId)
    {
        table = FindUserTable(table);
        var columns = GetEntryColumns(table);
        string? key = columns.FirstOrDefault(c => c.AutoId)?.Name ??
            new[] { "rowid", "_rowid_", "oid" }.FirstOrDefault(alias => columns.All(c => !SchemaFields.SameName(c.Name, alias)));
        if (key == null) return Execute(SandboxExamples.ReadTable(table));

        string keySql = "t." + SchemaSql.Quote(key);
        string tableSql = "main." + SchemaSql.Quote(table);
        // Limit the secondary scan before ordering the preview, even for a large table.
        string query = $"""
            WITH page(k, priority) AS (
                SELECT ?1, 0
                UNION ALL
                SELECT previous.k, 1 FROM (
                    SELECT {keySql} AS k FROM {tableSql} AS t
                    WHERE {keySql} <> ?1 ORDER BY {keySql} LIMIT {DisplayRows}
                ) AS previous
            )
            SELECT t.* FROM page JOIN {tableSql} AS t ON {keySql} = page.k
            ORDER BY page.priority, page.k;
            """;
        return ExecuteCore(query, bind: stmt => Check(sqlite3_bind_int64(stmt, 1, rowId)));
    }

    private static EntryInputException InvalidValue(EntryColumn field) =>
        new(field, $"Enter a valid {field.Type} value for '{field.Name}'.");

    private void BindEntry(IntPtr stmt, int index, EntryColumn field, string? text)
    {
        if (text == null)
        {
            if (field.Required && !field.AutoId) throw new EntryInputException(field, $"'{field.Name}' requires a value.");
            Check(sqlite3_bind_null(stmt, index));
            return;
        }

        switch (field.Type)
        {
            case "INTEGER":
                if (text.Contains('\0') || !long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long integer))
                    throw InvalidValue(field);
                Check(sqlite3_bind_int64(stmt, index, integer));
                break;
            case "REAL":
            case "NUMERIC":
                if (text.Contains('\0') || !double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number) ||
                    !double.IsFinite(number)) throw InvalidValue(field);
                if (field.Type == "REAL") Check(sqlite3_bind_double(stmt, index, number));
                // Let SQLite apply NUMERIC affinity to the original literal (an integer can exceed binary64 precision).
                else BindText(stmt, index, text.Trim());
                break;
            case "TEXT":
                BindText(stmt, index, text);
                break;
            case "BLOB":
                byte[] bytes;
                try { bytes = Convert.FromHexString(text); }
                catch (FormatException) { throw InvalidValue(field); }
                Check(bytes.Length == 0 ? sqlite3_bind_zeroblob(stmt, index, 0) :
                    sqlite3_bind_blob(stmt, index, bytes, bytes.Length, Transient));
                break;
            default:
                throw InvalidValue(field);
        }
    }

    private void BindText(IntPtr stmt, int index, string text)
    {
        // A non-null buffer distinguishes the empty string from NULL. UTF-8 preserves a leading U+FEFF.
        var bytes = new byte[Encoding.UTF8.GetByteCount(text) + 1];
        int size = Encoding.UTF8.GetBytes(text, bytes);
        Check(sqlite3_bind_text(stmt, index, bytes, size, Transient));
    }
}

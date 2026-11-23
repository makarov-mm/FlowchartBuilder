using System.Diagnostics;
using System.Runtime.InteropServices;
using static Makarov.FlowchartBuilder.Tools.EntityRelationship.SandboxNative;

namespace Makarov.FlowchartBuilder.Tools.EntityRelationship;

internal sealed record SandboxResult(IReadOnlyList<string> Columns, IReadOnlyList<string?[]> Rows, int Changes, bool MoreRows);

internal sealed partial class SandboxDatabase : IDisposable
{
    internal const int DisplayRows = 200;
    private const int SqliteDeny = 1, SqliteRow = 100, SqliteDone = 101;
    private enum ActionCode
    {
        Delete = 9, Insert = 18, Pragma = 19, Read = 20, Select = 21, Update = 23, Function = 31, Recursive = 33
    }

    private static readonly HashSet<string> AllowedFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "abs", "avg", "cast", "changes", "char", "coalesce", "concat", "count", "date", "datetime", "dense_rank",
        "first_value", "glob", "group_concat", "hex", "ifnull", "iif", "instr", "julianday", "lag", "last_insert_rowid",
        "last_value", "lead", "length", "like", "likelihood", "likely", "lower", "ltrim", "max", "min", "nullif",
        "printf", "quote", "random", "rank", "replace", "round", "row_number", "rtrim", "sign", "sqlite_version",
        "strftime", "substr", "substring", "sum", "time", "total", "total_changes", "trim", "typeof", "unicode",
        "unixepoch", "unlikely", "upper", "zeroblob", "current_timestamp", "current_date", "current_time"
    };

    private readonly Dictionary<string, string> _tables = new(StringComparer.Ordinal);
    internal IEnumerable<string> TableNames => _tables.Values.Order(StringComparer.Ordinal);
    private readonly Authorize _authorize;
    private readonly Progress _progress;
    private readonly Stopwatch _clock = new();
    private IntPtr _database;
    private bool _runningUserSql;

    public SandboxDatabase(string generatedSchema)
    {
        _authorize = AuthorizeQuery;
        _progress = _ => _runningUserSql && _clock.ElapsedMilliseconds >= 1500 ? 1 : 0;
        try
        {
            Check(sqlite3_open(":memory:", out _database));
            Run("PRAGMA temp_store = MEMORY; PRAGMA foreign_keys = ON; PRAGMA max_page_count = 8192;");
            Run(generatedSchema);
            // Collect from sqlite_master directly, including names beyond the result grid's display limit.
            Run("SELECT name FROM sqlite_master WHERE type = 'table';", statement =>
            {
                string table = Cell(statement, 0, truncate: false)!;
                _tables.Add(SchemaFields.NameKey(table), table);
            });
            sqlite3_limit(_database, 0, 1_000_000); // SQLITE_LIMIT_LENGTH (per string, BLOB or row)
            sqlite3_limit(_database, 1, 1_000_000); // SQLITE_LIMIT_SQL_LENGTH
            Check(sqlite3_set_authorizer(_database, _authorize, IntPtr.Zero));
            sqlite3_progress_handler(_database, 1000, _progress, IntPtr.Zero);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public SandboxResult Execute(string sql) => ExecuteCore(sql);

    private SandboxResult ExecuteCore(string sql, Action<IntPtr>? visitRow = null, Action<IntPtr>? bind = null)
    {
        ObjectDisposedException.ThrowIf(_database == IntPtr.Zero, this);
        int before = sqlite3_total_changes(_database);
        Run("BEGIN");
        try
        {
            _clock.Restart();
            _runningUserSql = true;
            SandboxResult result;
            try { result = Run(sql, visitRow, bind); }
            finally { _runningUserSql = false; _clock.Stop(); }
            Run("COMMIT");
            return result with { Changes = sqlite3_total_changes(_database) - before };
        }
        catch
        {
            // Some SQLite failures roll the transaction back themselves.
            if (sqlite3_get_autocommit(_database) == 0) Run("ROLLBACK");
            throw;
        }
    }

    private int AuthorizeQuery(IntPtr context, int action, IntPtr first, IntPtr second, IntPtr db, IntPtr trigger)
    {
        if (!_runningUserSql) return 0;
        string name = SchemaFields.NameKey(Marshal.PtrToStringUTF8(first) ?? "");
        bool allowed = (ActionCode)action switch
        {
            ActionCode.Read => _tables.ContainsKey(name) || name is "sqlite_master" or "sqlite_schema",
            ActionCode.Insert or ActionCode.Update or ActionCode.Delete => _tables.ContainsKey(name),
            ActionCode.Select or ActionCode.Recursive => true,
            ActionCode.Function => AllowedFunctions.Contains(Marshal.PtrToStringUTF8(second) ?? ""),
            ActionCode.Pragma => name switch
            {
                "table_info" or "index_list" or "index_info" or "foreign_key_list" => true,
                "foreign_keys" => second == IntPtr.Zero,
                _ => false
            },
            _ => false
        };
        return allowed ? 0 : SqliteDeny;
    }

    private SandboxResult Run(string sql, Action<IntPtr>? visitRow = null, Action<IntPtr>? bind = null)
    {
        if (sql.Contains('\0')) throw new ArgumentException("SQL contains a null character.");
        string[] columns = [];
        var rows = new List<string?[]>();
        bool moreRows = false;
        IntPtr buffer = Marshal.StringToHGlobalUni(sql);
        try
        {
            IntPtr cursor = buffer;
            IntPtr end = buffer + checked(sql.Length * sizeof(char));
            while (cursor != end)
            {
                Check(sqlite3_prepare16_v2(_database, cursor, checked((int)(end - cursor)), out var statement, out var tail));
                cursor = tail;
                if (statement == IntPtr.Zero) continue; // Whitespace or comment.
                try
                {
                    bind?.Invoke(statement);
                    int width = sqlite3_column_count(statement);
                    if (width > 0 && visitRow == null)
                    {
                        columns = Enumerable.Range(0, width).Select(i => Marshal.PtrToStringUni(sqlite3_column_name16(statement, i)) ?? "").ToArray();
                        rows.Clear();
                        moreRows = false;
                    }
                    int status;
                    while ((status = sqlite3_step(statement)) == SqliteRow)
                    {
                        if (visitRow != null) visitRow(statement);
                        else if (rows.Count < DisplayRows)
                        {
                            var row = new string?[width];
                            for (int i = 0; i < width; i++) row[i] = Cell(statement, i, truncate: true);
                            rows.Add(row);
                        }
                        else moreRows = true;
                    }
                    if (status != SqliteDone) Check(status);
                }
                finally { sqlite3_finalize(statement); }
            }
        }
        finally { Marshal.FreeHGlobal(buffer); }
        return new(columns, rows, 0, moreRows);
    }

    private static string? Cell(IntPtr statement, int column, bool truncate)
    {
        int type = sqlite3_column_type(statement, column);
        if (type == 5) return null; // SQLITE_NULL
        if (type == 4) return $"BLOB ({sqlite3_column_bytes(statement, column)} B)";
        var value = sqlite3_column_text16(statement, column);
        int length = sqlite3_column_bytes16(statement, column) / sizeof(char);
        const int previewChars = 1024;
        if (truncate && length > previewChars) return Marshal.PtrToStringUni(value, previewChars) + "…";
        return Marshal.PtrToStringUni(value, length);
    }

    private void Check(int status)
    {
        if (status == 0) return;
        string message = Marshal.PtrToStringUTF8(sqlite3_errmsg(_database)) ?? $"SQLite error {status}";
        throw new InvalidOperationException(message);
    }

    public void Dispose()
    {
        if (_database == IntPtr.Zero) return;
        sqlite3_close(_database);
        _database = IntPtr.Zero;
    }
}

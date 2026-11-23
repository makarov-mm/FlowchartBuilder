using System.Text.RegularExpressions;

namespace Makarov.FlowchartBuilder.Tools.EntityRelationship;

public sealed record SchemaField(string Name, string Type, bool PrimaryKey = false, bool Nullable = true,
    bool Unique = false, string? UniqueWhenNull = null);
public sealed record SchemaTable(string Id, string Name, IReadOnlyList<SchemaField> Fields);
public sealed record SchemaRelation(string Id, string ParentTableId, string ChildTableId, string ChildField);
public sealed record SchemaDiagnostic(string? Id, string Message, string Russian, string Ukrainian)
{
    public string GetMessage(string language) => language switch
    {
        "Russian" => Russian, "Ukrainian" => Ukrainian, _ => Message
    };
}

public sealed record SchemaResult(string? Sql, IReadOnlyList<SchemaDiagnostic> Errors)
{
    public bool IsValid => Sql != null;
}

public static partial class SchemaFields
{
    // Field names follow SQLite's double-quote convention. Tokens following the name
    // are checked separately and never passed through to SQL.
    [GeneratedRegex("""^(?:"(?<quoted>(?:""|[^"])*)"|(?<name>[^\s"]+))\s+(?<type>\S+)(?:\s+(?<flags>.+))?$""")]
    private static partial Regex FieldLine();

    public static IReadOnlyList<SchemaField> Parse(string tableId, string text, List<SchemaDiagnostic> errors)
    {
        var fields = new List<SchemaField>();
        using var reader = new StringReader(text);
        int lineNumber = 0;
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            line = line.Trim();
            if (line.Length == 0) continue;
            var match = FieldLine().Match(line);
            if (!match.Success)
            {
                errors.Add(new(tableId, $"Field line {lineNumber} needs a name and a type (e.g. id INTEGER PK).",
                    $"В строке {lineNumber} укажите имя и тип поля (например, id INTEGER PK).",
                    $"У рядку {lineNumber} зазначте назву й тип поля (наприклад, id INTEGER PK)."));
                continue;
            }
            var name = match.Groups["quoted"].Success
                ? match.Groups["quoted"].Value.Replace("\"\"", "\"")
                : match.Groups["name"].Value;
            var flags = match.Groups["flags"].Value.ToUpperInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            bool primary = false, required = false, unique = false;
            for (int i = 0; i < flags.Length; i++)
            {
                string flag = flags[i];
                if ((flag == "NOT" || flag == "PRIMARY") && i + 1 < flags.Length)
                    flag += " " + flags[++i];
                switch (flag)
                {
                    case "PK" or "PRIMARY KEY" when !primary: primary = true; break;
                    case "NOT NULL" when !required: required = true; break;
                    case "UNIQUE" when !unique: unique = true; break;
                    default:
                        errors.Add(new(tableId, $"Unknown or repeated option '{flag}' on field line {lineNumber}. Use PK, NOT NULL or UNIQUE.",
                            $"Неизвестная или повторная опция «{flag}» в строке {lineNumber}. Допустимы PK, NOT NULL и UNIQUE.",
                            $"Невідома або повторна опція «{flag}» у рядку {lineNumber}. Допустимі PK, NOT NULL і UNIQUE."));
                        break;
                }
            }
            fields.Add(new(name, match.Groups["type"].Value.ToUpperInvariant(), primary, !required && !primary, unique));
        }
        return fields;
    }

    // SQLite folds only ASCII letters when matching identifiers.
    internal static string NameKey(string value) => string.Create(value.Length, value, static (span, source) =>
    {
        for (int i = 0; i < source.Length; i++)
            span[i] = char.IsAsciiLetterUpper(source[i]) ? char.ToLowerInvariant(source[i]) : source[i];
    });

    public static bool SameName(string first, string second) => NameKey(first) == NameKey(second);
}

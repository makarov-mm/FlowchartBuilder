using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Glyphs.EntityRelationship;

namespace Makarov.FlowchartBuilder.Tools.EntityRelationship;

internal static class EntityDiagramReader
{
    public static SchemaResult Analyze(Document document, out bool libraryExample)
    {
        var errors = new List<SchemaDiagnostic>();
        var glyphs = document.DocumentSheet.Glyphs;
        var tables = glyphs.OfType<TableGlyph>().Select(table => ReadTable(table, errors)).ToArray();
        var relations = new List<SchemaRelation>();
        foreach (var connector in glyphs.OfType<ConnectorGlyph>())
        {
            if (connector is RelationshipGlyph edge)
                relations.Add(new(edge.Id, edge.Source?.Id ?? "", edge.Target?.Id ?? "", edge.ChildField));
            else if (connector.Source is TableGlyph && connector.Target is TableGlyph)
                errors.Add(new(connector.Id, "Connect the tables with the One-to-many relationship tool.",
                    "Соедините таблицы инструментом «Связь один ко многим».",
                    "З’єднайте таблиці інструментом «Зв’язок один до багатьох»."));
        }

        libraryExample = SupportsLibraryQueries(tables);
        var result = SchemaCompiler.Compile(tables, relations);
        if (errors.Count == 0) return result;
        errors.AddRange(result.Errors);
        return new(null, errors);
    }

    private static SchemaTable ReadTable(TableGlyph table, List<SchemaDiagnostic> errors)
    {
        var fields = SchemaFields.Parse(table.Id, table.Fields, errors).ToArray();
        string unique = table.UniqueField.Trim(), condition = table.WhileNullField.Trim();
        if (unique.Length > 0 || condition.Length > 0)
        {
            int index = Array.FindIndex(fields, field => SchemaFields.SameName(field.Name, unique));
            if (index < 0 || condition.Length == 0)
                errors.Add(new(table.Id,
                    "For conditional uniqueness, choose an existing unique field and a nullable condition field.",
                    "Для условной уникальности укажите существующее поле уникальности и поле, которое может быть NULL.",
                    "Для умовної унікальності вкажіть наявне поле унікальності й поле, що може мати значення NULL."));
            else fields[index] = fields[index] with { UniqueWhenNull = condition };
        }
        return new(table.Id, table.Caption, fields);
    }

    // The walkthrough uses these columns explicitly. It also works on a renamed copy of the example.
    private static bool SupportsLibraryQueries(IEnumerable<SchemaTable> tables)
    {
        bool Has(string name, params string[] fields) => tables.Any(t =>
            SchemaFields.SameName(t.Name, name) &&
            fields.All(required => t.Fields.Any(f => SchemaFields.SameName(f.Name, required))));
        return Has("books", "id", "title") && Has("copies", "id", "book_id") &&
               Has("readers", "id", "name") && Has("loans", "id", "copy_id", "reader_id", "issued_on", "due_on", "returned_on");
    }
}

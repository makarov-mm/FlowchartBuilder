namespace Makarov.FlowchartBuilder.Tools.EntityRelationship;

internal static class SchemaSql
{
    internal static string Quote(string identifier) => "\"" + identifier.Replace("\"", "\"\"") + "\"";

    internal static string Generate(IReadOnlyDictionary<string, SchemaTable> byId, IReadOnlyList<SchemaRelation> relations)
    {
        var sql = new List<string> { "PRAGMA foreign_keys = ON;", "", "BEGIN;", "" };
        var tables = byId.Values.OrderBy(t => t.Name, StringComparer.Ordinal).ToArray();
        var byChild = relations.ToLookup(r => r.ChildTableId, StringComparer.Ordinal);
        foreach (var table in tables)
        {
            var columns = new List<string>();
            foreach (var field in table.Fields)
            {
                var column = $"    {Quote(field.Name)} {SchemaCompiler.TypeName(field)}";
                if (field.PrimaryKey) column += " PRIMARY KEY";
                if (field.PrimaryKey || !field.Nullable) column += " NOT NULL";
                if (field.Unique && !field.PrimaryKey) column += " UNIQUE";
                columns.Add(column);
            }
            foreach (var relation in byChild[table.Id].OrderBy(r => r.ChildField, StringComparer.Ordinal))
            {
                var parent = byId[relation.ParentTableId];
                var key = parent.Fields.Single(f => f.PrimaryKey);
                var childKey = table.Fields.Single(f => SchemaFields.SameName(f.Name, relation.ChildField));
                columns.Add($"    FOREIGN KEY ({Quote(childKey.Name)}) REFERENCES {Quote(parent.Name)} ({Quote(key.Name)})");
            }
            sql.Add($"CREATE TABLE {Quote(table.Name)} (");
            sql.Add(string.Join(",\n", columns));
            sql.Add(");");
            sql.Add("");
        }

        // Indexes and tables share a namespace in SQLite. Different table/field pairs
        // can also yield the same name when a user includes underscores in either.
        var usedNames = tables.Select(t => SchemaFields.NameKey(t.Name)).ToHashSet(StringComparer.Ordinal);
        foreach (var table in tables)
        {
            foreach (var field in table.Fields.Where(f => f.UniqueWhenNull != null))
            {
                string baseName = $"ux_{table.Name}_{field.Name}_open", indexName = baseName;
                int suffix = 2;
                while (!usedNames.Add(SchemaFields.NameKey(indexName))) indexName = $"{baseName}_{suffix++}";
                var condition = table.Fields.Single(f => SchemaFields.SameName(f.Name, field.UniqueWhenNull!));
                sql.Add($"CREATE UNIQUE INDEX {Quote(indexName)} ON {Quote(table.Name)} ({Quote(field.Name)})");
                sql.Add($"    WHERE {Quote(condition.Name)} IS NULL;");
                sql.Add("");
            }
        }
        sql.Add("COMMIT;");
        sql.Add("");
        return string.Join("\n", sql);
    }
}

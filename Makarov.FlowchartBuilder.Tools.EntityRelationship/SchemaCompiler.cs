namespace Makarov.FlowchartBuilder.Tools.EntityRelationship;

public static class SchemaCompiler
{
    public static SchemaResult Compile(IReadOnlyList<SchemaTable> tables, IReadOnlyList<SchemaRelation> relations)
    {
        var errors = new List<SchemaDiagnostic>();
        var byId = new Dictionary<string, SchemaTable>(StringComparer.Ordinal);
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var names = new HashSet<string>(StringComparer.Ordinal);
        if (tables.Count == 0)
            errors.Add(new(null, "Add a table to the diagram.", "Добавьте таблицу на диаграмму.", "Додайте таблицю до діаграми."));

        foreach (var table in tables)
        {
            if (CheckId(table.Id, ids, errors)) byId.Add(table.Id, table);
            if (!ValidName(table.Name) || SchemaFields.NameKey(table.Name).StartsWith("sqlite_", StringComparison.Ordinal))
                errors.Add(new(table.Id, "Enter a table name without control characters or the reserved sqlite_ prefix.",
                    "Введите имя таблицы без управляющих символов и зарезервированного префикса sqlite_.",
                    "Введіть назву таблиці без керівних символів і зарезервованого префікса sqlite_."));
            else if (!names.Add(SchemaFields.NameKey(table.Name)))
                errors.Add(new(table.Id, $"Duplicate table name '{table.Name}'.",
                    $"Имя таблицы «{table.Name}» повторяется.", $"Назва таблиці «{table.Name}» повторюється."));
            CheckFields(table, errors);
        }

        var usedChildFields = new HashSet<(string, string)>();
        foreach (var relation in relations)
        {
            CheckId(relation.Id, ids, errors);
            if (!byId.TryGetValue(relation.ParentTableId, out var parent) ||
                !byId.TryGetValue(relation.ChildTableId, out var child))
            {
                errors.Add(new(relation.Id, "Connect the relationship from the parent table to the child table.",
                    "Соедините родительскую таблицу с дочерней направленной связью.",
                    "З’єднайте батьківську таблицю з дочірньою напрямленим зв’язком."));
                continue;
            }
            var field = child.Fields.FirstOrDefault(f => SchemaFields.SameName(f.Name, relation.ChildField));
            if (field == null)
            {
                errors.Add(new(relation.Id, $"Select a foreign key field in '{child.Name}'.",
                    $"Укажите существующее поле внешнего ключа таблицы «{child.Name}».",
                    $"Зазначте наявне поле зовнішнього ключа таблиці «{child.Name}»."));
                continue;
            }
            if (!usedChildFields.Add((child.Id, SchemaFields.NameKey(field.Name))))
                errors.Add(new(relation.Id, $"Field '{child.Name}.{field.Name}' already has a relationship.",
                    $"Для поля «{child.Name}.{field.Name}» уже задана связь.",
                    $"Для поля «{child.Name}.{field.Name}» вже задано зв’язок."));
            var key = parent.Fields.FirstOrDefault(f => f.PrimaryKey);
            if (key == null)
                errors.Add(new(relation.Id, $"Mark a primary key (PK) in the parent table '{parent.Name}'.",
                    $"Отметьте первичный ключ (PK) в родительской таблице «{parent.Name}».",
                    $"Позначте первинний ключ (PK) у батьківській таблиці «{parent.Name}»."));
            else if (TypeName(field) != TypeName(key))
                errors.Add(new(relation.Id, $"Field '{child.Name}.{field.Name}' must match the type of '{parent.Name}.{key.Name}'.",
                    $"Тип поля «{child.Name}.{field.Name}» должен совпадать с типом «{parent.Name}.{key.Name}».",
                    $"Тип поля «{child.Name}.{field.Name}» має збігатися з типом «{parent.Name}.{key.Name}»."));
        }

        return new(errors.Count == 0 ? SchemaSql.Generate(byId, relations) : null, errors);
    }

    private static bool ValidName(string name) => !string.IsNullOrWhiteSpace(name) && !name.Any(char.IsControl);
    internal static string TypeName(SchemaField field) => field.Type.Trim().ToUpperInvariant();

    private static bool CheckId(string id, HashSet<string> ids, List<SchemaDiagnostic> errors)
    {
        if (!string.IsNullOrWhiteSpace(id) && ids.Add(id)) return true;
        errors.Add(new(id, "The element ID is missing or repeated.", "Идентификатор элемента отсутствует или повторяется.",
            "Ідентифікатор елемента відсутній або повторюється."));
        return false;
    }

    private static void CheckFields(SchemaTable table, List<SchemaDiagnostic> errors)
    {
        if (table.Fields.Count == 0)
            errors.Add(new(table.Id, $"Add a field to '{table.Name}'.", $"Добавьте поле в таблицу «{table.Name}».",
                $"Додайте поле до таблиці «{table.Name}»."));
        if (table.Fields.Count(f => f.PrimaryKey) > 1)
            errors.Add(new(table.Id, $"Select a single primary key (PK) in '{table.Name}'.",
                $"Выберите один первичный ключ (PK) в таблице «{table.Name}».",
                $"Виберіть один первинний ключ (PK) у таблиці «{table.Name}»."));
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var field in table.Fields)
        {
            if (!ValidName(field.Name))
                errors.Add(new(table.Id, $"Enter a field name without control characters in '{table.Name}'.",
                    $"Укажите имя поля без управляющих символов в таблице «{table.Name}».",
                    $"Зазначте назву поля без керівних символів у таблиці «{table.Name}»."));
            else if (!names.Add(SchemaFields.NameKey(field.Name)))
                errors.Add(new(table.Id, $"Duplicate field '{field.Name}' in '{table.Name}'.",
                    $"Поле «{field.Name}» повторяется в таблице «{table.Name}».",
                    $"Поле «{field.Name}» повторюється в таблиці «{table.Name}»."));
            if (TypeName(field) is not ("INTEGER" or "REAL" or "TEXT" or "BLOB" or "NUMERIC"))
                errors.Add(new(table.Id, $"Set the type of '{table.Name}.{field.Name}' to INTEGER, REAL, TEXT, BLOB or NUMERIC.",
                    $"Задайте для «{table.Name}.{field.Name}» тип INTEGER, REAL, TEXT, BLOB или NUMERIC.",
                    $"Задайте для «{table.Name}.{field.Name}» тип INTEGER, REAL, TEXT, BLOB або NUMERIC."));
            if (field.UniqueWhenNull is not { } condition) continue;
            var other = table.Fields.FirstOrDefault(f => SchemaFields.SameName(f.Name, condition));
            if (field.PrimaryKey || field.Unique)
                errors.Add(new(table.Id, $"Conditional uniqueness of '{table.Name}.{field.Name}' conflicts with its PK or UNIQUE option.",
                    $"Условная уникальность «{table.Name}.{field.Name}» несовместима с PK или UNIQUE для этого поля.",
                    $"Умовна унікальність «{table.Name}.{field.Name}» несумісна з PK або UNIQUE для цього поля."));
            if (other == null || other == field || other.PrimaryKey || !other.Nullable)
                errors.Add(new(table.Id, $"The NULL condition for '{table.Name}.{field.Name}' must name another nullable field in this table.",
                    $"Для условия NULL поля «{table.Name}.{field.Name}» укажите другое поле этой таблицы, допускающее NULL.",
                    $"Для умови NULL поля «{table.Name}.{field.Name}» зазначте інше поле цієї таблиці, яке допускає NULL."));
        }
    }
}

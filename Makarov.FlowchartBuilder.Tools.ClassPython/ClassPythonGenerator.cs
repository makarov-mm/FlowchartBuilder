using System.Text;

namespace Makarov.FlowchartBuilder.Tools.ClassPython;

internal sealed record ClassDefinition(string Id, string Name, string Attributes, string Methods);
internal enum ClassRelationKind { Inheritance, Association }
internal sealed record ClassRelation(string Id, ClassRelationKind Kind, string SourceId, string TargetId, string Role);
internal sealed record ClassIssue(string? GlyphId, string English, string Russian, string Ukrainian)
{
    public string Message(string language) => language switch { "Russian" => Russian, "Ukrainian" => Ukrainian, _ => English };
}
internal sealed record ClassExportResult(string Code, IReadOnlyList<ClassIssue> Issues);

internal static class ClassPythonGenerator
{
    private sealed class Model(ClassDefinition definition)
    {
        public string Id => definition.Id;
        public string Name { get; } = definition.Name.Trim();
        public List<ClassField> Fields { get; } = [];
        public List<ClassMethod> Methods { get; } = [];
        public Dictionary<string, (string GlyphId, bool IsMethod)> Members { get; } = new(StringComparer.Ordinal);
        public Model? Parent { get; set; }
        public string? ParentEdgeId { get; set; }

        public bool AddMember(string name, string glyphId, bool isMethod, List<ClassIssue> issues)
        {
            if (Members.TryAdd(name, (glyphId, isMethod))) return true;
            issues.Add(new(glyphId, $"Duplicate member '{Name}.{name}'.", $"Имя члена «{Name}.{name}» повторяется.",
                $"Ім’я члена «{Name}.{name}» повторюється."));
            return false;
        }
    }

    public static ClassExportResult Generate(IReadOnlyList<ClassDefinition> classes, IReadOnlyList<ClassRelation> relations)
    {
        var issues = new List<ClassIssue>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        var byId = new Dictionary<string, Model>(StringComparer.Ordinal);
        var models = new List<Model>();
        if (classes.Count == 0)
            issues.Add(new(null, "Add a UML class.", "Добавьте UML-класс.", "Додайте UML-клас."));
        foreach (var definition in classes)
        {
            var model = new Model(definition);
            models.Add(model);
            if (!ClassSignatures.IsClassName(model.Name))
                issues.Add(new(model.Id, $"Invalid or reserved Python class name '{model.Name}'.",
                    $"Недопустимое или зарезервированное имя Python-класса «{model.Name}».",
                    $"Неприпустиме або зарезервоване ім’я Python-класу «{model.Name}»."));
            else if (!names.Add(model.Name))
                issues.Add(new(model.Id, $"Duplicate class '{model.Name}'.", $"Имя класса «{model.Name}» повторяется.",
                    $"Ім’я класу «{model.Name}» повторюється."));
            if (string.IsNullOrWhiteSpace(model.Id) || !byId.TryAdd(model.Id, model))
                issues.Add(new(model.Id, "Class ID is missing or repeated.", "Идентификатор класса отсутствует или повторяется.",
                    "Ідентифікатор класу відсутній або повторюється."));
        }

        for (int i = 0; i < classes.Count; i++)
        {
            ReadMembers(models[i], classes[i].Attributes, names, issues, methods: false);
            ReadMembers(models[i], classes[i].Methods, names, issues, methods: true);
        }
        foreach (var relation in relations.OrderBy(r => r.Role, StringComparer.Ordinal))
        {
            if (!byId.TryGetValue(relation.SourceId, out var source) || !byId.TryGetValue(relation.TargetId, out var target))
            {
                issues.Add(new(relation.Id, "Connect both ends of this relationship to UML classes.",
                    "Соедините оба конца связи с UML-классами.", "З’єднайте обидва кінці зв’язку з UML-класами."));
                continue;
            }
            if (relation.Kind == ClassRelationKind.Inheritance)
            {
                if (source.Parent != null)
                    issues.Add(new(relation.Id, $"'{source.Name}' can have only one base class.",
                        $"У класса «{source.Name}» может быть только один базовый класс.",
                        $"Клас «{source.Name}» може мати лише один базовий клас."));
                else
                {
                    source.Parent = target;
                    source.ParentEdgeId = relation.Id;
                }
            }
            else
            {
                string role = relation.Role.Trim();
                if (!ClassSignatures.IsMemberName(role))
                    issues.Add(new(relation.Id, "Give the association a valid Python field name.",
                        "Задайте для ассоциации допустимое имя поля Python.", "Задайте для асоціації допустиме ім’я поля Python."));
                else if (source.AddMember(role, relation.Id, false, issues))
                    source.Fields.Add(new(role, target.Name));
            }
        }

        var ordered = OrderByInheritance(models, issues);
        if (issues.Count == 0) CheckInheritedMembers(ordered, issues);
        return new(issues.Count == 0 ? Render(ordered) : string.Empty, issues);
    }

    private static void ReadMembers(Model model, string text, IReadOnlySet<string> names, List<ClassIssue> issues, bool methods)
    {
        using var reader = new StringReader(text);
        int number = 0;
        while (reader.ReadLine() is { } line)
        {
            number++;
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parser = new ClassSignatures(line, names);
            try
            {
                if (methods)
                {
                    var method = parser.Method();
                    if (model.AddMember(method.Name, model.Id, true, issues)) model.Methods.Add(method);
                }
                else
                {
                    var field = parser.Field();
                    if (model.AddMember(field.Name, model.Id, false, issues)) model.Fields.Add(field);
                }
            }
            catch (FormatException)
            {
                issues.Add(methods
                    ? new(model.Id, $"Invalid method on line {number} of '{model.Name}'. Use method(arg: type) -> type.",
                        $"Ошибка в строке {number} методов класса «{model.Name}». Формат: method(arg: type) -> type.",
                        $"Помилка в рядку {number} методів класу «{model.Name}». Формат: method(arg: type) -> type.")
                    : new(model.Id, $"Invalid field on line {number} of '{model.Name}'. Use name: type.",
                        $"Ошибка в строке {number} полей класса «{model.Name}». Формат: name: type.",
                        $"Помилка в рядку {number} полів класу «{model.Name}». Формат: name: type."));
            }
        }
    }

    private static List<Model> OrderByInheritance(List<Model> models, List<ClassIssue> issues)
    {
        var ordered = new List<Model>();
        var placed = new HashSet<Model>();
        foreach (var start in models.OrderBy(c => c.Name, StringComparer.Ordinal))
        {
            var path = new List<Model>();
            var seen = new HashSet<Model>();
            for (var node = start; node != null && !placed.Contains(node); node = node.Parent)
            {
                if (!seen.Add(node))
                {
                    foreach (var member in path.Skip(path.IndexOf(node)))
                        issues.Add(new(member.ParentEdgeId, $"Inheritance cycle at '{member.Name}'.",
                            $"Цикл наследования у класса «{member.Name}».", $"Цикл успадкування у класу «{member.Name}»."));
                    break;
                }
                path.Add(node);
            }
            for (int i = path.Count - 1; i >= 0; i--)
            {
                placed.Add(path[i]);
                ordered.Add(path[i]);
            }
        }
        return ordered;
    }

    private static void CheckInheritedMembers(List<Model> models, List<ClassIssue> issues)
    {
        foreach (var model in models)
            foreach (var (name, member) in model.Members)
                for (var parent = model.Parent; parent != null; parent = parent.Parent)
                {
                    if (!parent.Members.TryGetValue(name, out var inherited)) continue;
                    if (!(member.IsMethod && inherited.IsMethod))
                        issues.Add(new(member.GlyphId, $"'{model.Name}.{name}' conflicts with a member of '{parent.Name}'.",
                            $"«{model.Name}.{name}» конфликтует с членом базового класса «{parent.Name}».",
                            $"«{model.Name}.{name}» конфліктує з членом базового класу «{parent.Name}»."));
                    break;
                }
    }

    private static string Render(List<Model> models)
    {
        var code = new StringBuilder();
        code.AppendLine("from __future__ import annotations").AppendLine()
            .AppendLine("from dataclasses import dataclass as _dataclass");
        foreach (var model in models)
        {
            string bases = model.Parent == null ? "" : $"({model.Parent.Name})";
            code.AppendLine().AppendLine().AppendLine("@_dataclass").AppendLine($"class {model.Name}{bases}:");
            foreach (var field in model.Fields) code.AppendLine($"    {field.Name}: {field.Type}");
            if (model.Fields.Count == 0 && model.Methods.Count == 0) code.AppendLine("    pass");
            for (int i = 0; i < model.Methods.Count; i++)
            {
                if (i > 0 || model.Fields.Count > 0) code.AppendLine();
                var method = model.Methods[i];
                string args = string.Join(", ", method.Parameters.Select(p => $"{p.Name}: {p.Type}").Prepend("self"));
                code.AppendLine($"    def {method.Name}({args}) -> {method.Result}:")
                    .AppendLine("        raise NotImplementedError");
            }
        }
        return code.ToString();
    }
}

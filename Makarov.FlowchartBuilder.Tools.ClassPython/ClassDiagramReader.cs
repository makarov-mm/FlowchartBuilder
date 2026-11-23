using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Glyphs.Uml;

namespace Makarov.FlowchartBuilder.Tools.ClassPython;

internal static class ClassDiagramReader
{
    public static ClassExportResult Generate(Document document)
    {
        var glyphs = document.DocumentSheet.Glyphs;
        var classes = glyphs.OfType<ClassGlyph>()
            .Select(glyph => new ClassDefinition(glyph.Id, glyph.Caption.Trim(), glyph.Attributes, glyph.Methods))
            .ToList();
        var relations = new List<ClassRelation>();
        var issues = new List<ClassIssue>();

        foreach (var link in glyphs.OfType<ConnectorGlyph>())
        {
            ClassRelationKind? kind = link switch
            {
                InheritanceGlyph => ClassRelationKind.Inheritance,
                AssociationGlyph => ClassRelationKind.Association,
                _ => null
            };
            if (kind.HasValue)
            {
                relations.Add(new ClassRelation(link.Id, kind.Value,
                    link.Source?.Id ?? "", link.Target?.Id ?? "", link.Caption.Trim()));
            }
            else if (link.Source is ClassGlyph && link.Target is ClassGlyph)
            {
                issues.Add(new(link.Id, "Use the UML association or inheritance tool for a relationship between classes.",
                    "Для связи между классами выберите в палитре UML ассоциацию или наследование.",
                    "Для зв’язку між класами виберіть на палітрі UML асоціацію або успадкування."));
            }
        }

        var result = ClassPythonGenerator.Generate(classes, relations);
        if (issues.Count == 0) return result;
        issues.AddRange(result.Issues);
        return new ClassExportResult(string.Empty, issues);
    }
}

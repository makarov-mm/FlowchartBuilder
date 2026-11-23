using System.Text.RegularExpressions;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Tools.PythonExport;

internal sealed record FlowchartAnalysis(
    IReadOnlyList<FlowchartDiagnostic> Diagnostics,
    IReadOnlyList<BlockGlyph> Order,
    IReadOnlyDictionary<BlockGlyph, BlockGlyph[]> Successors,
    IReadOnlyDictionary<BlockGlyph, string> Statements);

internal sealed record FlowchartDiagnostic(string? GlyphId, string English, string Russian, string Ukrainian)
{
    public string Message(string language) => language switch
    {
        "Russian" => Russian, "Ukrainian" => Ukrainian, _ => English
    };
}

internal static class FlowchartValidator
{
    private static readonly HashSet<string> TrueLabels = new(StringComparer.OrdinalIgnoreCase) { "true", "yes", "да", "так", "1" };
    private static readonly HashSet<string> FalseLabels = new(StringComparer.OrdinalIgnoreCase) { "false", "no", "нет", "ні", "0" };
    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "False", "None", "True", "and", "as", "assert", "async", "await", "break", "class", "continue",
        "def", "del", "elif", "else", "except", "finally", "for", "from", "global", "if", "import", "in",
        "is", "lambda", "nonlocal", "not", "or", "pass", "raise", "return", "try", "while", "with", "yield"
    };

    public static FlowchartAnalysis Analyze(Document document)
    {
        var glyphs = document.DocumentSheet.Glyphs;
        var nodes = glyphs.OfType<BlockGlyph>().Where(block => block.Family == "Flowchart").ToList();
        var outgoing = nodes.ToDictionary(block => block, _ => new List<ConnectorGlyph>());
        var starts = nodes.Where(node => node.Name == "Begin").ToArray();
        var diagnostics = new List<FlowchartDiagnostic>();

        if (starts.Length == 0)
            diagnostics.Add(new(null, "Add a Begin block.", "Добавьте блок начала.", "Додайте блок початку."));
        if (starts.Length > 1)
            foreach (var start in starts)
                diagnostics.Add(new(start.Id, "Keep exactly one Begin block.", "Оставьте один блок начала.", "Залиште один блок початку."));
        if (!nodes.Any(node => node.Name == "End"))
            diagnostics.Add(new(null, "Add an End block.", "Добавьте блок конца.", "Додайте блок кінця."));

        foreach (var edge in glyphs.OfType<ConnectorGlyph>())
        {
            bool from = edge.Source is not null && outgoing.ContainsKey(edge.Source);
            bool to = edge.Target is not null && outgoing.ContainsKey(edge.Target);
            if (!from && !to) continue;
            if (!from || !to)
            {
                diagnostics.Add(new(edge.Id, "Connect both ends to flowchart blocks.",
                    "Соедините этой стрелкой два блока алгоритма.", "З’єднайте цією стрілкою два блоки алгоритму."));
                continue;
            }
            outgoing[edge.Source!].Add(edge);
            if (edge.Target!.Name == "Begin")
                diagnostics.Add(new(edge.Id, "Remove the connection entering Begin.",
                    "Уберите стрелку, входящую в начало.", "Приберіть стрілку, що входить у початок."));
        }

        var successors = new Dictionary<BlockGlyph, BlockGlyph[]>();
        var statements = new Dictionary<BlockGlyph, string>();
        foreach (var node in nodes)
        {
            List<ConnectorGlyph> edges = outgoing[node];
            int expected = node.Name switch { "If" => 2, "End" => 0, _ => 1 };
            if (edges.Count != expected)
                diagnostics.Add(new(node.Id, $"Outgoing connections: {edges.Count}. This block needs {expected}.",
                    $"Исходящих стрелок: {edges.Count}. У этого блока должно быть {expected}.",
                    $"Вихідних стрілок: {edges.Count}. Цей блок має мати {expected}."));
            if (node.Name == "If")
                edges = CheckBranches(edges, diagnostics);

            successors[node] = edges.Select(edge => edge.Target!).ToArray();
            if (node.Name is not ("Begin" or "End"))
                CheckStatement(node, statements, diagnostics);
        }

        var order = new List<BlockGlyph>();
        if (starts.Length == 1)
        {
            order.Add(starts[0]);
            var reached = new HashSet<BlockGlyph>(order);
            for (int i = 0; i < order.Count; i++)
                foreach (var target in successors[order[i]])
                    if (reached.Add(target)) order.Add(target);
            foreach (var node in nodes.Where(node => !reached.Contains(node)))
                diagnostics.Add(new(node.Id, "This block is unreachable from Begin.",
                    "В этот блок нельзя попасть из начала.", "У цей блок неможливо потрапити з початку."));
        }
        return new(diagnostics, order, successors, statements);
    }

    private static List<ConnectorGlyph> CheckBranches(List<ConnectorGlyph> edges, List<FlowchartDiagnostic> diagnostics)
    {
        var yes = new List<ConnectorGlyph>();
        var no = new List<ConnectorGlyph>();
        foreach (var edge in edges)
        {
            string label = edge.Caption.Trim();
            if (TrueLabels.Contains(label)) yes.Add(edge);
            else if (FalseLabels.Contains(label)) no.Add(edge);
            else diagnostics.Add(new(edge.Id, "Label this branch true or false.",
                "Подпишите ветвь «да» или «нет».", "Підпишіть гілку «так» або «ні»."));
        }
        var conflicts = new List<ConnectorGlyph>();
        if (yes.Count > 1) conflicts.AddRange(yes);
        if (no.Count > 1) conflicts.AddRange(no);
        foreach (var edge in conflicts)
            diagnostics.Add(new(edge.Id, "Branches have the same meaning. Label one true and the other false.",
                "Ветви имеют одинаковое значение. Подпишите одну «да», другую «нет».",
                "Гілки мають однакове значення. Підпишіть одну «так», іншу «ні»."));
        return edges.Count == 2 && yes.Count == 1 && no.Count == 1 ? [yes[0], no[0]] : edges;
    }

    private static void CheckStatement(BlockGlyph node, Dictionary<BlockGlyph, string> statements, List<FlowchartDiagnostic> diagnostics)
    {
        string body = node.Caption.Trim();
        var translations = Core.Instance.Translators;
        if (body.Length == 0 || body.Contains('\n') || body.Contains('\r') || body == node.Name ||
            translations.Languages.Any(language => translations[language].Contains(node.Name) && translations[language][node.Name] == body))
        {
            diagnostics.Add(new(node.Id, "Enter a single-line Python caption for this block.",
                "Введите для блока однострочную подпись на Python.", "Введіть для блока однорядковий підпис мовою Python."));
            return;
        }

        switch (node.Name)
        {
            case "Operation":
            case "Subroutine":
            case "If":
                statements[node] = body;
                break;
            case "In/Out":
                if (body.StartsWith("output ", StringComparison.OrdinalIgnoreCase) && body[7..].Trim() is { Length: > 0 } expression)
                    statements[node] = $"print({expression})";
                else if (body.StartsWith("input ", StringComparison.OrdinalIgnoreCase) &&
                         Regex.IsMatch(body[6..].Trim(), @"^[A-Za-z_][A-Za-z_0-9]*$") && !Keywords.Contains(body[6..].Trim()))
                    statements[node] = $"{body[6..].Trim()} = input()";
                else
                    diagnostics.Add(new(node.Id, "Use 'input name' or 'output expression' in this block.",
                        "Используйте «input имя» или «output выражение».", "Використайте «input ім’я» або «output вираз»."));
                break;
            default:
                diagnostics.Add(new(node.Id, $"Unsupported block: {node.Name}.", $"Тип блока «{node.Name}» не поддерживается.",
                    $"Тип блока «{node.Name}» не підтримується."));
                break;
        }
    }
}

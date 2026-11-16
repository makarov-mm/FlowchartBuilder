using System.Text;
using System.Text.RegularExpressions;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Tools.PythonExport;

internal static class PythonGenerator
{
    private static readonly string[] TrueLabels = ["true", "yes", "да", "так", "1"];
    private static readonly string[] FalseLabels = ["false", "no", "нет", "ні", "0"];

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "False","None","True","and","as","assert","async","await","break","class","continue","def","del",
        "elif","else","except","finally","for","from","global","if","import","in","is","lambda","nonlocal",
        "not","or","pass","raise","return","try","while","with","yield"
    };

    public static string Generate(Document document)
    {
        var glyphs = document.DocumentSheet.Glyphs;
        var nodes = glyphs.OfType<BlockGlyph>().Where(b => b.Family == "Flowchart").ToHashSet();
        var starts = nodes.Where(b => b.Name == "Begin").ToArray();
        if (starts.Length != 1) throw new InvalidDataException("The flowchart needs exactly one Begin block.");
        if (!nodes.Any(b => b.Name == "End")) throw new InvalidDataException("Add an End block.");
        var outgoing = nodes.ToDictionary(b => b, _ => new List<ConnectorGlyph>());
        foreach (var edge in glyphs.OfType<ConnectorGlyph>())
        {
            bool from = edge.Source != null && nodes.Contains(edge.Source);
            bool to = edge.Target != null && nodes.Contains(edge.Target);
            if (!from && !to) continue;
            if (!(from && to)) throw new InvalidDataException("A flowchart connection has an endpoint outside the flowchart.");
            if (edge.Target == starts[0]) throw new InvalidDataException("Begin cannot have incoming connections.");
            outgoing[edge.Source!].Add(edge);
        }
        List<BlockGlyph> order = [starts[0]];
        var ids = new Dictionary<BlockGlyph, int> { [starts[0]] = 0 };
        var successors = new Dictionary<BlockGlyph, BlockGlyph[]>();
        for (int i = 0; i < order.Count; i++)
        {
            var block = order[i];
            var edges = outgoing[block];
            int count = block.Name switch { "If" => 2, "End" => 0, _ => 1 };
            if (edges.Count != count) throw new InvalidDataException($"{block.Name} '{block.Caption}' needs {count} outgoing connections (found {edges.Count}).");
            if (block.Name == "If")
            {
                var yes = edges.Where(e => TrueLabels.Contains(e.Caption.Trim(), StringComparer.OrdinalIgnoreCase)).ToArray();
                var no = edges.Where(e => FalseLabels.Contains(e.Caption.Trim(), StringComparer.OrdinalIgnoreCase)).ToArray();
                if (yes.Length != 1 || no.Length != 1)
                    throw new InvalidDataException($"Label the two outgoing connections of '{block.Caption}' true and false.");
                edges = [yes[0], no[0]];
            }
            var next = edges.Select(e => e.Target!).ToArray();
            successors[block] = next;
            foreach (var target in next)
                if (ids.TryAdd(target, ids.Count)) order.Add(target);
        }
        if (order.Count != nodes.Count) throw new InvalidDataException("Every flowchart block must be reachable from Begin.");
        var code = new StringBuilder();
        code.AppendLine("def main():");
        bool branching = order.Any(b => b.Name == "If");
        string state = "_flow_step";
        while (order.Any(b => b.Caption.Contains(state, StringComparison.Ordinal))) state += "_";
        if (branching)
        {
            code.AppendLine($"    {state} = 0");
            code.AppendLine("    while True:");
        }
        foreach (var block in order)
        {
            var next = successors[block];
            string indent = branching ? "            " : "    ";
            if (branching) code.AppendLine($"        {(ids[block] == 0 ? "if" : "elif")} {state} == {ids[block]}:");
            if (block.Name == "End") { code.AppendLine(indent + "return"); continue; }
            string body = block.Caption.Trim();
            if (block.Name != "Begin")
            {
                if (string.IsNullOrWhiteSpace(body) || body.Contains('\n') || body.Contains('\r') ||
                    body == block.Name || Core.Instance.Translators.Languages.Any(l => Core.Instance.Translators[l].Contains(block.Name) && Core.Instance.Translators[l][block.Name] == body))
                    throw new InvalidDataException($"Set a single-line Python caption on the {block.Name} block.");
                if (block.Name == "If")
                {
                    code.AppendLine($"{indent}{state} = {ids[next[0]]} if ({body}) else {ids[next[1]]}");
                    continue;
                }
                if (block.Name == "In/Out") body = InputOutput(body);
                else if (block.Name is not ("Operation" or "Subroutine"))
                    throw new InvalidDataException($"Unsupported block: {block.Name}.");
                code.AppendLine(indent + body);
            }
            if (branching) code.AppendLine($"{indent}{state} = {ids[next[0]]}");
        }
        code.AppendLine().AppendLine().AppendLine("if __name__ == \"__main__\":").AppendLine("    main()");
        return code.ToString();
    }

    private static string InputOutput(string caption)
    {
        if (caption.StartsWith("output ", StringComparison.OrdinalIgnoreCase) && caption[7..].Trim() is { Length: > 0 } expression)
            return $"print({expression})";
        if (caption.StartsWith("input ", StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(caption[6..].Trim(), @"^[A-Za-z_][A-Za-z_0-9]*$") && !Keywords.Contains(caption[6..].Trim()))
            return $"{caption[6..].Trim()} = input()";
        throw new InvalidDataException("Use 'input name' or 'output expression' in an In/Out block.");
    }
}

using System.Text;
using System.Text.RegularExpressions;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Tools.PythonExport;

internal sealed record PythonGenerationResult(string Code, IReadOnlyDictionary<int, string> LineToBlockId);

internal readonly record struct PythonLine(int Indent, string Text, string? BlockId = null);

internal static class PythonGenerator
{
    public static string Generate(Document document) => GenerateWithSourceMap(document).Code;

    public static PythonGenerationResult GenerateWithSourceMap(Document document) =>
        GenerateWithSourceMap(FlowchartValidator.Analyze(document));

    internal static PythonGenerationResult GenerateWithSourceMap(FlowchartAnalysis graph)
    {
        if (graph.Diagnostics.Count > 0)
            throw new InvalidDataException(graph.Diagnostics[0].Message("English"));

        List<PythonLine> lines = [new(0, "def main():", graph.Order[0].Id)];
        if (!graph.Order.Any(b => b.Name == "If"))
        {
            foreach (var block in graph.Order.Skip(1))
                lines.Add(new(1, block.Name == "End" ? "return" : graph.Statements[block], block.Id));
        }
        else
        {
            // A break or continue in a caption can explicitly target the dispatch loop.
            bool usesLoopControl = graph.Statements.Values.Any(s => Regex.IsMatch(s, @"\b(break|continue)\b"));
            var body = usesLoopControl ? null : StructuredFlowchart.TryBuild(graph);
            lines.AddRange(body ?? Dispatch(graph));
        }
        lines.Add(new(0, ""));
        lines.Add(new(0, ""));
        lines.Add(new(0, "if __name__ == \"__main__\":"));
        lines.Add(new(1, "main()"));

        var code = new StringBuilder();
        var sourceMap = new Dictionary<int, string>();
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            code.Append(' ', line.Indent * 4).AppendLine(line.Text);
            if (line.BlockId is { } id) sourceMap.Add(i + 1, id);
        }
        return new(code.ToString(), sourceMap);
    }

    private static List<PythonLine> Dispatch(FlowchartAnalysis graph)
    {
        string state = "_flow_step";
        while (graph.Order.Any(b => b.Caption.Contains(state, StringComparison.Ordinal))) state += "_";
        var ids = graph.Order.Select((block, index) => (block, index)).ToDictionary(p => p.block, p => p.index);
        List<PythonLine> body = [new(1, $"{state} = 0"), new(1, "while True:")];
        foreach (var block in graph.Order)
        {
            var next = graph.Successors[block];
            body.Add(new(2, $"{(ids[block] == 0 ? "if" : "elif")} {state} == {ids[block]}:", block.Id));
            switch (block.Name)
            {
                case "End":
                    body.Add(new(3, "return", block.Id));
                    break;
                case "If":
                    body.Add(new(3, $"{state} = {ids[next[0]]} if ({graph.Statements[block]}) else {ids[next[1]]}", block.Id));
                    break;
                default:
                    if (block.Name != "Begin") body.Add(new(3, graph.Statements[block], block.Id));
                    body.Add(new(3, $"{state} = {ids[next[0]]}", block.Id));
                    break;
            }
        }
        return body;
    }
}

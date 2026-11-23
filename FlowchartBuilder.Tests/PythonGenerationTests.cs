using Makarov.FlowchartBuilder;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Tools.PythonExport;
using Makarov.Framework.Core.SettingStorages;
using Xunit;

namespace FlowchartBuilder.Tests;

[Collection("Editor")]
public class PythonGenerationTests
{
    [Fact]
    public void DiamondEmitsOneSharedContinuationAndMapsBothArms()
    {
        var diagram = new Diagram();
        var start = diagram.Block("Begin");
        var decision = diagram.Block("If", "n > 0");
        var yes = diagram.Block("Operation", "sign = 1");
        var no = diagram.Block("Operation", "sign = 0");
        var output = diagram.Block("In/Out", "output sign");
        var end = diagram.Block("End");
        diagram.Link(start, decision);
        diagram.Link(decision, yes, "да");
        diagram.Link(decision, no, "нет");
        diagram.Link(yes, output);
        diagram.Link(no, output);
        diagram.Link(output, end);

        var result = PythonGenerator.GenerateWithSourceMap(diagram.Document);
        string[] lines = result.Code.Split(Environment.NewLine);
        string[] expected = ["def main():", "    if n > 0:", "        sign = 1", "    else:",
            "        sign = 0", "    print(sign)", "    return"];
        Assert.Equal(expected, lines.Take(expected.Length));
        Assert.Equal(new[] { start.Id, decision.Id, yes.Id, decision.Id, no.Id, output.Id, end.Id },
            Enumerable.Range(1, expected.Length).Select(n => result.LineToBlockId[n]));
        Assert.Equal(expected.Length, result.LineToBlockId.Count);
        Assert.Single(lines, line => line.Trim() == "print(sign)");
    }

    [Fact]
    public void FalseLoopArmCanContainAOneSidedDecision()
    {
        var diagram = new Diagram();
        var start = diagram.Block("Begin");
        var loop = diagram.Block("If", "done()");
        var decision = diagram.Block("If", "visible()");
        var output = diagram.Block("In/Out", "output 7");
        var update = diagram.Block("Operation", "advance()");
        var end = diagram.Block("End");
        diagram.Link(start, loop);
        diagram.Link(loop, end, "true");
        diagram.Link(loop, decision, "false");
        diagram.Link(decision, output, "true");
        diagram.Link(decision, update, "false");
        diagram.Link(output, update);
        diagram.Link(update, loop);

        string[] code = PythonGenerator.Generate(diagram.Document).Split(Environment.NewLine);
        string[] expected = ["def main():", "    while not (done()):", "        if visible():",
            "            print(7)", "        advance()", "    return"];
        Assert.Equal(expected, code.Take(expected.Length));
    }

    [Fact]
    public void TwoEntryLoopKeepsDispatchAndSourceMap()
    {
        var diagram = new Diagram();
        var start = diagram.Block("Begin");
        var entry = diagram.Block("If", "skip_first");
        var condition = diagram.Block("If", "more()");
        var body = diagram.Block("Operation", "_flow_step = value()");
        var end = diagram.Block("End");
        diagram.Link(start, entry);
        diagram.Link(entry, condition, "true");
        diagram.Link(entry, body, "false");
        diagram.Link(condition, body, "true");
        diagram.Link(condition, end, "false");
        diagram.Link(body, condition);

        var result = PythonGenerator.GenerateWithSourceMap(diagram.Document);
        string[] lines = result.Code.Split(Environment.NewLine);
        Assert.Contains("    _flow_step_ = 0", lines);
        Assert.Contains("            _flow_step_ = 2 if (skip_first) else 3", lines);
        Assert.Contains("            _flow_step_ = 3 if (more()) else 4", lines);
        int operation = Array.IndexOf(lines, "            _flow_step = value()");
        Assert.True(operation > 0);
        Assert.Equal(body.Id, result.LineToBlockId[operation + 1]);
        Assert.Equal("            _flow_step_ = 2", lines[operation + 1]);
    }

    private sealed class Diagram
    {
        private readonly Core _core = Core.Initialize(new MemoryStorage());
        public Document Document { get; } = new();

        public BlockGlyph Block(string name, string? caption = null)
        {
            var type = _core.GlyphTypes.Single(t => GlyphMetadata.Of(t).Name == name);
            var block = (BlockGlyph)Activator.CreateInstance(type)!;
            if (caption != null) block.Caption = caption;
            Document.DocumentSheet.Add(block);
            return block;
        }

        public void Link(BlockGlyph source, BlockGlyph target, string caption = "") =>
            Document.DocumentSheet.Add(new ConnectorGlyph(source, 2, target, 0) { Caption = caption });
    }
}

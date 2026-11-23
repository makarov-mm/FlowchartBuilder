using Makarov.FlowchartBuilder;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Tools.PythonExport;
using Makarov.Framework.Core.SettingStorages;
using Xunit;

namespace FlowchartBuilder.Tests;

[Collection("Editor")]
public class FlowchartValidationTests
{
    private static Document Countdown()
    {
        Core.Initialize(new MemoryStorage());
        return Document.Load(Path.Combine(AppContext.BaseDirectory, "Examples", "Countdown.mfb"));
    }

    [Fact]
    public void CheckReportsBothInvalidItemsAndKeepsTheirGlyphIdsAcrossUndo()
    {
        var document = Countdown();
        var arrow = document.DocumentSheet.Glyphs.OfType<ConnectorGlyph>().Single(g => g.Caption == "true");
        var output = document.DocumentSheet.Glyphs.Single(g => g.Name == "In/Out");
        arrow.Caption = "?";
        output.Caption = "input class";
        document.CommitChange();

        var problems = FlowchartValidator.Analyze(document).Diagnostics;
        Assert.Equal(2, problems.Count);
        var branch = Assert.Single(problems, issue => issue.GlyphId == arrow.Id);
        Assert.Single(problems, issue => issue.GlyphId == output.Id);
        Assert.Contains("true", branch.Message("English"));
        Assert.Contains("да", branch.Message("Russian"));
        Assert.Contains("так", branch.Message("Ukrainian"));

        int changes = 0;
        document.Changed += (_, _) => changes++;
        arrow.Caption = "так";
        output.Caption = "output n";
        Assert.True(document.CommitChange());
        Assert.False(document.CommitChange());
        Assert.Empty(FlowchartValidator.Analyze(document).Diagnostics);
        Assert.True(document.Undo());
        Assert.Equal(problems, FlowchartValidator.Analyze(document).Diagnostics);
        Assert.True(document.Redo());
        Assert.Empty(FlowchartValidator.Analyze(document).Diagnostics);
        Assert.Equal(3, changes);
    }

    [Fact]
    public void ExercisePointsToBothConflictingBranchesAndClearsAfterRepair()
    {
        Core.Initialize(new MemoryStorage());
        var document = Document.Load(Path.Combine(AppContext.BaseDirectory, "Examples", "Countdown-exercise.mfb"));
        var decision = document.DocumentSheet.Glyphs.Single(glyph => glyph.Name == "If");
        var branches = document.DocumentSheet.Glyphs.OfType<ConnectorGlyph>().Where(edge => edge.Source == decision).ToList();
        var problems = FlowchartValidator.Analyze(document).Diagnostics;
        Assert.Equal(2, problems.Count);
        Assert.Equal(branches.Select(edge => edge.Id).Order(), problems.Select(problem => problem.GlyphId).Order());
        branches.Single(edge => edge.Target!.Name == "End").Caption = "нет";
        Assert.Empty(FlowchartValidator.Analyze(document).Diagnostics);
    }

    [Fact]
    public void CheckDistinguishesAnUnreachableBlockFromAnEmptyDiagram()
    {
        var document = Countdown();
        var extra = (BlockGlyph)document.DocumentSheet.Glyphs.Single(g => g.Name == "End").Clone();
        document.DocumentSheet.Add(extra);
        var isolated = Assert.Single(FlowchartValidator.Analyze(document).Diagnostics);
        Assert.Equal(extra.Id, isolated.GlyphId);
        Assert.Contains("unreachable", isolated.Message("English"));

        var empty = FlowchartValidator.Analyze(new Document()).Diagnostics;
        Assert.Equal(2, empty.Count);
        Assert.All(empty, issue => Assert.Null(issue.GlyphId));
    }
}

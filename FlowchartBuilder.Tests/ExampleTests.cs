using Makarov.FlowchartBuilder;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.Framework.Core.SettingStorages;
using Xunit;

namespace FlowchartBuilder.Tests;

[Collection("Editor")]
public sealed class ExampleTests
{
    [Fact]
    public void OpeningAnExampleTwiceCreatesIndependentUntitledCopies()
    {
        Core.Initialize(new MemoryStorage());
        string source = Path.Combine(AppContext.BaseDirectory, "Examples", "Countdown.mfb");
        var first = Document.LoadExample(source);
        var second = Document.LoadExample(source);
        Assert.Null(first.FileName);
        Assert.Null(second.FileName);
        Assert.NotEqual(first.Id, second.Id);
        Assert.False(first.IsModified);

        first.DocumentSheet.Glyphs.Single(g => g.Id == "init").Caption = "n = 5";
        first.CommitChange();
        Assert.True(first.IsModified);
        Assert.Equal("n = 3", second.DocumentSheet.Glyphs.Single(g => g.Id == "init").Caption);

        string copy = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mfb");
        try
        {
            string? updatedTitle = null;
            first.Changed += (_, _) => updatedTitle = first.DisplayName;
            first.Save(copy);
            Assert.Equal(Path.GetFileNameWithoutExtension(copy), updatedTitle);
            Assert.Equal(copy, first.FileName);
            Assert.Equal("n = 5", Document.Load(copy).DocumentSheet.Glyphs.Single(g => g.Id == "init").Caption);
            Assert.Equal("n = 3", Document.Load(source).DocumentSheet.Glyphs.Single(g => g.Id == "init").Caption);
        }
        finally { File.Delete(copy); }
    }

    [Fact]
    public void CorrectingExerciseExitRestoresTheWorkingCountdown()
    {
        Core.Initialize(new MemoryStorage());
        string examples = Path.Combine(AppContext.BaseDirectory, "Examples");
        var exercise = Document.LoadExample(Path.Combine(examples, "Countdown-exercise.mfb"));
        Assert.Throws<InvalidDataException>(() => Makarov.FlowchartBuilder.Tools.PythonExport.PythonGenerator.Generate(exercise));
        exercise.DocumentSheet.Glyphs.OfType<ConnectorGlyph>().Single(e => e.Target?.Name == "End").Caption = "false";

        var solution = Document.LoadExample(Path.Combine(examples, "Countdown.mfb"));
        Assert.Equal(Makarov.FlowchartBuilder.Tools.PythonExport.PythonGenerator.Generate(solution),
            Makarov.FlowchartBuilder.Tools.PythonExport.PythonGenerator.Generate(exercise));
    }
}

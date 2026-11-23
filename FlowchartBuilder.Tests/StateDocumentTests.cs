using Makarov.FlowchartBuilder;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Glyphs.States;
using Makarov.FlowchartBuilder.Tools.StateMachine;
using Makarov.Framework.Core.SettingStorages;
using Xunit;

namespace FlowchartBuilder.Tests;

[Collection("Editor")]
public sealed class StateDocumentTests
{
    private const string Expire = "Истёк срок";
    private const string Return = "Книгу вернули";

    private static Document OpenBookLoan()
    {
        Core.Initialize(new MemoryStorage());
        return Document.LoadExample(Path.Combine(AppContext.BaseDirectory, "Examples", "Book-loan.mfb"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReturningTheBookClosesAnActiveOrOverdueLoan(bool overdue)
    {
        var analysis = StateDiagramReader.Analyze(OpenBookLoan());
        Assert.Empty(analysis.Issues);
        Assert.NotNull(analysis.Definition);
        var machine = analysis.Definition;
        string state = machine.Initial.Id;
        Assert.Equal("active", state);
        Assert.Equal(new[] { Expire, Return }.Order(StringComparer.Ordinal),
            machine.Available(state).Select(t => t.Event).Order(StringComparer.Ordinal));
        if (overdue)
        {
            Assert.True(machine.TryStep(state, Expire, out var transition));
            state = transition!.TargetId;
            Assert.Equal("overdue", state);
            Assert.Equal(Return, Assert.Single(machine.Available(state)).Event);
            Assert.False(machine.TryStep(state, Expire, out _));
        }
        Assert.True(machine.TryStep(state, Return, out var returned));
        Assert.Equal("closed", returned!.TargetId);
        Assert.Empty(machine.Available(returned.TargetId));
        Assert.False(machine.TryStep(returned.TargetId, Return, out _));
    }

    [Fact]
    public void SavedExampleRestoresStateFlagsEventTextAndAttachedPorts()
    {
        var document = OpenBookLoan();
        Assert.Null(document.FileName);
        var lateReturn = document.DocumentSheet.Glyphs.OfType<TransitionGlyph>().Single(g => g.Id == "return-late");
        lateReturn.Caption = "Вернули после срока";
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mfb");
        try
        {
            document.Save(path);
            var loaded = Document.Load(path);
            Assert.False(loaded.HasSkippedProperties);
            var states = loaded.DocumentSheet.Glyphs.OfType<StateGlyph>().ToDictionary(s => s.Id);
            Assert.Equal(3, states.Count);
            Assert.Equal("Активна", Assert.Single(states.Values, s => s.IsInitial).Caption);
            Assert.Equal("Закрыта", Assert.Single(states.Values, s => s.IsFinal).Caption);
            var transitions = loaded.DocumentSheet.Glyphs.OfType<TransitionGlyph>().ToDictionary(t => t.Id);
            Assert.Equal(3, transitions.Count);
            void Check(string id, string source, int sourcePort, string target, int targetPort, string caption)
            {
                var transition = transitions[id];
                Assert.Same(states[source], transition.Source);
                Assert.Equal(sourcePort, transition.SourcePort);
                Assert.Same(states[target], transition.Target);
                Assert.Equal(targetPort, transition.TargetPort);
                Assert.Equal(caption, transition.Caption);
                Assert.Equal(ConnectorRouting.Straight, transition.Routing);
            }
            Check("expire", "active", 1, "overdue", 3, Expire);
            Check("return-on-time", "active", 2, "closed", 3, Return);
            Check("return-late", "overdue", 2, "closed", 1, "Вернули после срока");

            var analysis = StateDiagramReader.Analyze(loaded);
            Assert.Empty(analysis.Issues);
            Assert.True(analysis.Definition!.TryStep("overdue", "Вернули после срока", out var step));
            Assert.Equal("closed", step!.TargetId);
            Assert.False(analysis.Definition.TryStep("overdue", Return, out _));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void EditorUndoAndRedoReanalyzeChangedTransitionsAndRegeneratePython()
    {
        var document = OpenBookLoan();
        var before = StateDiagramReader.Analyze(document);
        Assert.Empty(before.Issues);
        string originalCode = before.Definition!.GeneratePython();
        var expiry = document.DocumentSheet.Glyphs.OfType<TransitionGlyph>().Single(g => g.Id == "expire");
        expiry.Caption = Return;
        Assert.True(document.CommitChange());
        var ambiguous = StateDiagramReader.Analyze(document);
        Assert.NotEmpty(ambiguous.Issues);
        Assert.Null(ambiguous.Definition);

        Assert.True(document.Undo());
        var restored = StateDiagramReader.Analyze(document);
        Assert.Empty(restored.Issues);
        Assert.Equal(originalCode, restored.Definition!.GeneratePython());
        Assert.True(document.Redo());
        Assert.Null(StateDiagramReader.Analyze(document).Definition);

        var edited = document.DocumentSheet.Glyphs.OfType<TransitionGlyph>().Single(g => g.Id == "expire");
        edited.Caption = "Срок закончился";
        Assert.True(document.CommitChange());
        var changed = StateDiagramReader.Analyze(document);
        Assert.Empty(changed.Issues);
        Assert.False(changed.Definition!.TryStep("active", Expire, out _));
        Assert.True(changed.Definition.TryStep("active", edited.Caption, out var step));
        Assert.Equal("overdue", step!.TargetId);
        Assert.NotEqual(originalCode, changed.Definition.GeneratePython());
    }
}

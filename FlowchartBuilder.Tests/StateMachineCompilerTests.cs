using Makarov.FlowchartBuilder.Tools.StateMachine;
using Xunit;

namespace FlowchartBuilder.Tests;

public class StateMachineCompilerTests
{
    [Fact]
    public void AStateCanLoopAndAnEventCanBeReusedInAnotherState()
    {
        MachineState[] states = [new("a", "Ready", true, false), new("b", "Running", false, false), new("end", "Done", false, true)];
        MachineTransition[] edges = [new("go", "a", "b", " advance "), new("stop", "b", "end", "advance"),
            new("wait", "a", "a", "wait"), new("again", "b", "a", "back")];
        var analysis = StateMachineCompiler.Analyze(states, edges);
        Assert.Empty(analysis.Issues);
        var machine = Assert.IsType<MachineDefinition>(analysis.Definition);
        Assert.Equal("a", machine.Initial.Id);
        Assert.Equal(new[] { "advance", "wait" }, machine.Available("a").Select(t => t.Event));
        Assert.True(machine.TryStep("a", "wait", out var stay));
        Assert.Equal("a", stay!.TargetId);
        Assert.True(machine.TryStep("a", "advance", out var first));
        Assert.True(machine.TryStep(first!.TargetId, "back", out var back));
        Assert.Equal(machine.Initial.Id, back!.TargetId);
        Assert.True(machine.TryStep(first.TargetId, "advance", out var last));
        Assert.True(machine.State(last!.TargetId).IsFinal);
        Assert.Empty(machine.Available("end"));
        Assert.False(machine.TryStep("end", "advance", out var invalid));
        Assert.Null(invalid);
        Assert.False(machine.TryStep("unknown", "advance", out _));
    }

    [Fact]
    public void AnOrdinaryDeadEndAndAnInitialFinalStateAreValid()
    {
        foreach (bool isFinal in new[] { false, true })
        {
            var result = StateMachineCompiler.Analyze([new("only", "Only", true, isFinal)], []);
            Assert.Empty(result.Issues);
            Assert.Empty(result.Definition!.Available("only"));
        }
    }

    [Fact]
    public void ReportsAmbiguousEventsUnreachableStatesFinalAndMissingEndpoints()
    {
        MachineState[] states = [new("a", "First", true, false), new("b", "Last", false, true), new("u", "Disconnected", false, false)];
        MachineTransition[] edges = [new("go", "a", "b", "run"), new("amb", "a", "a", " run "),
            new("final", "b", "a", "again"), new("empty", "a", "b", " "), new("broken", "a", "", "lost")];
        var analysis = StateMachineCompiler.Analyze(states, edges);
        Assert.Null(analysis.Definition);
        Assert.Equal(new[] { "amb", "broken", "empty", "final", "u" },
            analysis.Issues.Select(i => i.GlyphId).Order(StringComparer.Ordinal));
        foreach (var issue in analysis.Issues)
        {
            Assert.NotEqual(issue.English, issue.Message("Russian"));
            Assert.NotEqual(issue.Russian, issue.Message("Ukrainian"));
        }
    }

    [Fact]
    public void RejectsRepeatedNamesMissingOrDuplicateIdsAndMultipleInitialStates()
    {
        MachineState[] states = [new("first", " Alpha ", true, false), new("second", "Alpha", true, false),
            new("first", "", false, false), new("", "Third", false, false)];
        var result = StateMachineCompiler.Analyze(states, [new("second", "first", "second", "event")]);
        Assert.Null(result.Definition);
        Assert.Contains(result.Issues, i => i.GlyphId == "second" && i.English.Contains("Duplicate state name"));
        Assert.Equal(2, result.Issues.Count(i => i.English.Contains("Only one state")));
        Assert.Equal(3, result.Issues.Count(i => i.English.Contains("ID is missing or repeated")));
        Assert.Contains(result.Issues, i => i.GlyphId == "first" && i.English.Contains("Give this state a name"));
        Assert.Null(StateMachineCompiler.Analyze([], []).Definition);
        Assert.Null(StateMachineCompiler.Analyze([new("x", "X", false, false)], []).Definition);
    }

    [Fact]
    public void PythonEnumNamesAndQuotedLabelsAreDeterministic()
    {
        MachineState[] states = [new("init", "GreenLight", true, false), new("word", "class", false, false),
            new("upper", "STOP", false, false), new("lower", "Stop", false, false), new("uni", "🚦 «Конец»", false, true)];
        MachineTransition[] edges = [new("one", "init", "word", "a \"quote\""), new("two", "word", "upper", "a\\b"),
            new("three", "upper", "lower", "a\nb"), new("four", "lower", "uni", "пуск 🚀")];
        var code = StateMachineCompiler.Analyze(states, edges).Definition!.GeneratePython();
        Assert.Contains("GREEN_LIGHT = \"GreenLight\"", code);
        Assert.DoesNotContain("STOP = ", code);
        Assert.DoesNotContain("CLASS = ", code);
        Assert.Contains("\"🚦 «Конец»\"", code);
        Assert.Contains("\"a \\\"quote\\\"\"", code);
        Assert.Contains("\"a\\\\b\"", code);
        Assert.Contains("\"a\\nb\"", code);
        Assert.Equal(code, StateMachineCompiler.Analyze(states.Reverse().ToArray(), edges.Reverse().ToArray()).Definition!.GeneratePython());
    }

    [Fact]
    public void LoanStatesKeepReadableCyrillicPythonIdentifiers()
    {
        MachineState[] states = [new("active", "Активна", true, false), new("overdue", "Просрочена", false, false),
            new("closed", "Закрыта", false, true)];
        MachineTransition[] edges = [new("expire", "active", "overdue", "Истёк срок"),
            new("return-on-time", "active", "closed", "Книгу вернули"), new("return-late", "overdue", "closed", "Книгу вернули")];
        var code = StateMachineCompiler.Analyze(states, edges).Definition!.GeneratePython();
        Assert.Contains("АКТИВНА = \"Активна\"", code);
        Assert.Contains("ПРОСРОЧЕНА = \"Просрочена\"", code);
        Assert.Contains("ЗАКРЫТА = \"Закрыта\"", code);
        Assert.Contains("\"Истёк срок\": State.ПРОСРОЧЕНА", code);
        var ukrainian = StateMachineCompiler.Analyze([new("uk", "Її ґанок", true, false)], []).Definition!.GeneratePython();
        Assert.Contains("ЇЇ_ҐАНОК = \"Її ґанок\"", ukrainian);
    }
}

using Makarov.FlowchartBuilder.Tools.StateMachine;
using Xunit;

namespace FlowchartBuilder.Tests;

public class MachineRunTests
{
    private static MachineDefinition Loan(string middleName = "Overdue", string expireEvent = "expire")
    {
        MachineState[] states = [new("a", "Active", true, false), new("o", middleName, false, false), new("c", "Closed", false, true)];
        MachineTransition[] edges = [new("expire", "a", "o", expireEvent), new("return", "a", "c", "return"),
            new("late return", "o", "c", "return")];
        return StateMachineCompiler.Analyze(states, edges).Definition!;
    }

    [Fact]
    public void ValidEditsKeepTheWalkAndIncompatibleEditsRestartIt()
    {
        var run = new MachineRun(Loan());
        Assert.True(run.Step("expire"));
        Assert.False(run.Step("expire"));
        run.Update(Loan(middleName: "Past due"));
        Assert.Equal("Past due", run.Current.Name);
        Assert.Equal("expire", Assert.Single(run.History).Id);
        run.Update(Loan(expireEvent: "past deadline"));
        Assert.Equal("a", run.Current.Id);
        Assert.Empty(run.History);
        Assert.True(run.Step("return"));
        Assert.Equal("c", run.Current.Id);
        Assert.False(run.Step("return"));
        run.Reset();
        Assert.Equal("a", run.Current.Id);
        Assert.Empty(run.History);
    }

    [Fact]
    public void UpdatingTheDiagramCannotSilentlyRedirectEarlierEvents()
    {
        var run = new MachineRun(Loan());
        Assert.True(run.Step("expire"));
        Assert.True(run.Step("return"));
        MachineState[] revised = [new("a", "Active", true, false), new("o", "Overdue", false, false), new("c", "Closed", false, true)];
        MachineTransition[] edges = [new("expire", "a", "c", "expire"), new("other", "a", "o", "check"), new("late return", "o", "c", "return")];
        run.Update(StateMachineCompiler.Analyze(revised, edges).Definition!);
        Assert.Equal("a", run.Current.Id);
        Assert.Empty(run.History);
    }

    [Fact]
    public void SteppingBackAfterAnEditRestoresTheUpdatedStateAndLetsTheRunBranch()
    {
        var run = new MachineRun(Loan());
        Assert.False(run.StepBack());
        Assert.True(run.Step("expire"));
        Assert.True(run.Step("return"));
        run.Update(Loan(middleName: "Past due"));
        Assert.True(run.StepBack());
        Assert.Equal("Past due", run.Current.Name);
        Assert.Equal("expire", Assert.Single(run.History).Id);
        Assert.Equal("return", Assert.Single(run.Definition.Available(run.Current.Id)).Event);
        Assert.True(run.StepBack());
        Assert.Equal("a", run.Current.Id);
        Assert.False(run.StepBack());
        Assert.True(run.Step("return"));
        Assert.Equal("return", Assert.Single(run.History).Id);
        Assert.Equal("c", run.Current.Id);
        run.Reset();
        Assert.False(run.StepBack());
    }

    [Fact]
    public void EachStepBackRemovesOneVisitEvenWhenAStateRepeats()
    {
        MachineState[] states = [new("a", "Available", true, false), new("l", "Loaned", false, false)];
        MachineTransition[] edges = [new("borrow", "a", "l", "borrow"), new("renew", "l", "l", "renew"),
            new("return", "l", "a", "return")];
        var run = new MachineRun(StateMachineCompiler.Analyze(states, edges).Definition!);
        foreach (string ev in new[] { "borrow", "renew", "return" }) Assert.True(run.Step(ev));
        foreach (var (id, remaining) in new[] { ("l", 2), ("l", 1), ("a", 0) })
        {
            Assert.True(run.StepBack());
            Assert.Equal(id, run.Current.Id);
            Assert.Equal(remaining, run.History.Count);
        }
        Assert.False(run.StepBack());
    }
}

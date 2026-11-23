namespace Makarov.FlowchartBuilder.Tools.StateMachine;

/// <summary>A manual run. Its current state and history belong to the tool window, outside the document.</summary>
internal sealed class MachineRun(MachineDefinition definition)
{
    private readonly List<MachineTransition> _history = [];
    public MachineDefinition Definition { get; private set; } = definition;
    public MachineState Current { get; private set; } = definition.Initial;
    public IReadOnlyList<MachineTransition> History => _history;

    public bool Step(string eventName)
    {
        if (!Definition.TryStep(Current.Id, eventName, out var transition) || transition == null) return false;
        _history.Add(transition);
        Current = Definition.State(transition.TargetId);
        return true;
    }

    public bool StepBack()
    {
        if (_history.Count == 0) return false;
        Current = Definition.State(_history[^1].SourceId);
        _history.RemoveAt(_history.Count - 1);
        return true;
    }

    public void Reset()
    {
        _history.Clear();
        Current = Definition.Initial;
    }

    // Revalidate the path after edits or undo; moving or renaming a state keeps the run.
    public void Update(MachineDefinition updated)
    {
        var previous = _history.ToArray();
        Definition = updated;
        Reset();
        foreach (var step in previous)
        {
            if (step.SourceId != Current.Id || !updated.TryStep(Current.Id, step.Event, out var next) ||
                next == null || next.Id != step.Id || next.TargetId != step.TargetId)
            {
                Reset();
                return;
            }
            _history.Add(next);
            Current = updated.State(next.TargetId);
        }
    }
}

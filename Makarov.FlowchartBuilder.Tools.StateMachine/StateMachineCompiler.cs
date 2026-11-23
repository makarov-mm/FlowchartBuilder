namespace Makarov.FlowchartBuilder.Tools.StateMachine;

internal sealed record MachineState(string Id, string Name, bool IsInitial, bool IsFinal);
internal sealed record MachineTransition(string Id, string SourceId, string TargetId, string Event);
internal sealed record MachineIssue(string? GlyphId, string English, string Russian, string Ukrainian)
{
    public string Message(string language) => language switch
    {
        "Russian" => Russian, "Ukrainian" => Ukrainian, _ => English
    };
}
internal sealed record MachineAnalysis(MachineDefinition? Definition, IReadOnlyList<MachineIssue> Issues);

internal static class StateMachineCompiler
{
    public static MachineAnalysis Analyze(IReadOnlyList<MachineState> states, IReadOnlyList<MachineTransition> transitions)
    {
        var issues = new List<MachineIssue>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var names = new HashSet<string>(StringComparer.Ordinal);
        var byId = new Dictionary<string, MachineState>(StringComparer.Ordinal);
        var initial = new List<MachineState>();
        foreach (var original in states)
        {
            var state = original with { Name = original.Name.Trim() };
            if (CheckId(state.Id, ids, issues)) byId.Add(state.Id, state);
            if (state.IsInitial) initial.Add(state);
            if (state.Name.Length == 0)
                issues.Add(new(state.Id, "Give this state a name.", "Задайте имя состояния.", "Задайте назву стану."));
            else if (!names.Add(state.Name))
                issues.Add(new(state.Id, $"Duplicate state name '{state.Name}'.", $"Имя состояния «{state.Name}» повторяется.",
                    $"Назва стану «{state.Name}» повторюється."));
        }
        if (initial.Count == 0)
            issues.Add(new(null, "Mark one state as initial.", "Отметьте одно состояние как начальное.", "Позначте один стан як початковий."));
        else if (initial.Count > 1)
            foreach (var state in initial)
                issues.Add(new(state.Id, "Only one state can be initial.", "Начальным может быть только одно состояние.",
                    "Початковим може бути лише один стан."));

        var outgoing = byId.Keys.ToDictionary(id => id, _ => new List<MachineTransition>(), StringComparer.Ordinal);
        var events = new HashSet<(string SourceId, string Event)>();
        foreach (var original in transitions)
        {
            var edge = original with { Event = original.Event.Trim() };
            CheckId(edge.Id, ids, issues);
            if (edge.Event.Length == 0)
                issues.Add(new(edge.Id, "Give the transition an event name.", "Задайте имя события перехода.",
                    "Задайте назву події переходу."));
            if (!byId.TryGetValue(edge.SourceId, out var source) || !byId.ContainsKey(edge.TargetId))
            {
                issues.Add(new(edge.Id, "Connect both ends of the transition to states.", "Соедините оба конца перехода с состояниями.",
                    "З’єднайте обидва кінці переходу зі станами."));
                continue;
            }
            if (source.IsFinal)
                issues.Add(new(edge.Id, $"Final state '{source.Name}' cannot have outgoing transitions.",
                    $"Из конечного состояния «{source.Name}» не должно быть переходов.",
                    $"З кінцевого стану «{source.Name}» не має бути переходів."));
            if (edge.Event.Length > 0 && !events.Add((source.Id, edge.Event)))
                issues.Add(new(edge.Id, $"State '{source.Name}' already has a transition for '{edge.Event}'.",
                    $"В состоянии «{source.Name}» уже есть переход для события «{edge.Event}».",
                    $"У стані «{source.Name}» уже є перехід для події «{edge.Event}»."));
            outgoing[source.Id].Add(edge);
        }
        if (initial.Count == 1 && states.Count == byId.Count)
        {
            var reachable = new HashSet<string>(StringComparer.Ordinal) { initial[0].Id };
            var queue = new Queue<string>();
            queue.Enqueue(initial[0].Id);
            while (queue.TryDequeue(out var id))
                foreach (var edge in outgoing[id])
                    if (reachable.Add(edge.TargetId)) queue.Enqueue(edge.TargetId);
            foreach (var state in byId.Values.Where(s => !reachable.Contains(s.Id)))
                issues.Add(new(state.Id, $"State '{state.Name}' is unreachable from the initial state.",
                    $"Состояние «{state.Name}» недостижимо из начального.", $"Стан «{state.Name}» недосяжний із початкового."));
        }
        if (issues.Count > 0) return new(null, issues);
        return new(new MachineDefinition(initial[0], byId, outgoing), issues);
    }

    private static bool CheckId(string id, HashSet<string> ids, List<MachineIssue> issues)
    {
        if (!string.IsNullOrWhiteSpace(id) && ids.Add(id)) return true;
        issues.Add(new(id, "The element ID is missing or repeated.", "Идентификатор элемента отсутствует или повторяется.",
            "Ідентифікатор елемента відсутній або повторюється."));
        return false;
    }
}

internal sealed class MachineDefinition
{
    private readonly IReadOnlyDictionary<string, MachineState> _states;
    private readonly Dictionary<string, IReadOnlyList<MachineTransition>> _outgoing;

    internal MachineDefinition(MachineState initial, Dictionary<string, MachineState> states, Dictionary<string, List<MachineTransition>> outgoing)
    {
        Initial = initial;
        _states = states;
        _outgoing = outgoing.ToDictionary(p => p.Key, p => (IReadOnlyList<MachineTransition>)
            Array.AsReadOnly(p.Value.OrderBy(t => t.Event, StringComparer.Ordinal).ToArray()), StringComparer.Ordinal);
    }

    public MachineState Initial { get; }
    public MachineState State(string id) => _states[id];
    public IReadOnlyList<MachineTransition> Available(string stateId) => _outgoing[stateId];

    public bool TryStep(string stateId, string eventName, out MachineTransition? transition)
    {
        transition = _outgoing.TryGetValue(stateId, out var available)
            ? available.FirstOrDefault(t => string.Equals(t.Event, eventName, StringComparison.Ordinal)) : null;
        return transition != null;
    }

    public string GeneratePython() => StateMachinePython.Generate(this, _states.Values);
}

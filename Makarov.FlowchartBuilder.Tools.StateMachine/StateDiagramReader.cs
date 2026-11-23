using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Glyphs.States;

namespace Makarov.FlowchartBuilder.Tools.StateMachine;

internal static class StateDiagramReader
{
    public static MachineAnalysis Analyze(Document document)
    {
        var glyphs = document.DocumentSheet.Glyphs;
        var states = glyphs.OfType<StateGlyph>().Select(state =>
            new MachineState(state.Id, state.Caption, state.IsInitial, state.IsFinal)).ToArray();
        var transitions = new List<MachineTransition>();
        var issues = new List<MachineIssue>();
        foreach (var connector in glyphs.OfType<ConnectorGlyph>())
        {
            if (connector is TransitionGlyph edge)
                transitions.Add(new(edge.Id, edge.Source?.Id ?? "", edge.Target?.Id ?? "", edge.Caption));
            else if (connector.Source is StateGlyph && connector.Target is StateGlyph)
                issues.Add(new(connector.Id, "Use the Transition tool to connect states.",
                    "Соедините состояния инструментом «Переход».", "З’єднайте стани інструментом «Перехід»."));
        }

        var analysis = StateMachineCompiler.Analyze(states, transitions);
        if (issues.Count == 0) return analysis;
        issues.AddRange(analysis.Issues);
        return new(null, issues);
    }
}

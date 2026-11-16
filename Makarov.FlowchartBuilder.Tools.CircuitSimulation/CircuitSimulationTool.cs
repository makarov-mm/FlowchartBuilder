namespace Makarov.FlowchartBuilder.Tools.CircuitSimulation;

public sealed class CircuitSimulationTool : IDocumentTool
{
    public string ExampleFileName => "RC-low-pass.mfb";

    public string Title(string language) => language switch
    {
        "Russian" => "Моделирование цепи…", "Ukrainian" => "Моделювання кола…", _ => "Circuit simulation…"
    };

    public void Run(Document document, IWin32Window owner, string language)
    {
        var model = CircuitSimulator.Build(CircuitNetwork.From(document));
        using var form = new SimulationForm(model, language) { Text = Title(language) };
        form.ShowDialog(owner);
    }
}

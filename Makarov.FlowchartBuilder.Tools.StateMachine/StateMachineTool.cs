namespace Makarov.FlowchartBuilder.Tools.StateMachine;

public sealed class StateMachineTool : IDocumentTool
{
    public string ExampleFileName => "Book-loan.mfb";

    public string Title(string language) => language switch
    {
        "Russian" => "Диаграмма состояний…",
        "Ukrainian" => "Діаграма станів…",
        _ => "State diagram…"
    };

    public void Run(Document document, IWin32Window owner, string language)
    {
        var open = Application.OpenForms.OfType<MachineForm>().FirstOrDefault(form => form.Document == document);
        if (open != null) open.Activate();
        else new MachineForm(document, language, Title(language)).Show(owner);
    }
}

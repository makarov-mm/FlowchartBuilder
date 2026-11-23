namespace Makarov.FlowchartBuilder.Tools.PythonExport;

public sealed class PythonExportTool : IDocumentTool
{
    public string ExampleFileName => "Countdown.mfb";

    public string Title(string language) => language switch
    {
        "Russian" => "Блок-схема → Python…", "Ukrainian" => "Блок-схема → Python…", _ => "Flowchart → Python…"
    };

    public void Run(Document document, IWin32Window owner, string language)
    {
        var existing = Application.OpenForms.OfType<PythonPreviewForm>().FirstOrDefault(form => form.Document == document);
        if (existing != null)
        {
            existing.Activate();
            return;
        }

        var preview = new PythonPreviewForm(document, language, Title(language));
        preview.Show(owner);
    }
}

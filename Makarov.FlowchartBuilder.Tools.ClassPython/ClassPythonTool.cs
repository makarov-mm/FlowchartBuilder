namespace Makarov.FlowchartBuilder.Tools.ClassPython;

public sealed class ClassPythonTool : IDocumentTool
{
    public string ExampleFileName => "Library-classes.mfb";

    public string Title(string language) => language switch
    {
        "Russian" => "Диаграмма классов → Python…",
        "Ukrainian" => "Діаграма класів → Python…",
        _ => "Class diagram → Python…"
    };

    public void Run(Document document, IWin32Window owner, string language)
    {
        var existing = Application.OpenForms.OfType<ClassPreviewForm>().FirstOrDefault(form => form.Document == document);
        if (existing != null) existing.Activate();
        else new ClassPreviewForm(document, language, Title(language)).Show(owner);
    }
}

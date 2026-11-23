namespace Makarov.FlowchartBuilder.Tools.EntityRelationship;

public sealed class EntityRelationshipTool : IDocumentTool
{
    public string ExampleFileName => "Library-database.mfb";

    public string Title(string language) => language switch
    {
        "Russian" => "ER-диаграмма → SQLite…",
        "Ukrainian" => "ER-діаграма → SQLite…",
        _ => "ER diagram → SQLite…"
    };

    public void Run(Document document, IWin32Window owner, string language)
    {
        var window = Application.OpenForms.OfType<EntityDiagramForm>().FirstOrDefault(form => form.Document == document);
        if (window == null) new EntityDiagramForm(document, language, Title(language)).Show(owner);
        else window.Activate();
    }
}

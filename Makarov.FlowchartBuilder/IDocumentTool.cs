namespace Makarov.FlowchartBuilder;

/// <summary>A command supplied by an optional assembly in Plugins. The editor owns the document and window.</summary>
public interface IDocumentTool
{
    string Title(string language);
    string? ExampleFileName => null;
    string? ExampleInstructions(string language) => null;
    void Run(Document document, IWin32Window owner, string language);
}

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Saves the current document; a document that was never saved asks for a file name.</summary>
public sealed class SaveCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.HasDocuments)
            Core.Instance.SaveDocument(Core.Instance.CurrentDocument);
    }
}

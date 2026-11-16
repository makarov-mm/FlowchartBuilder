namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Closes the current document, offering to save its changes.</summary>
public sealed class CloseCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.HasDocuments)
            Core.Instance.CloseDocument(Core.Instance.CurrentDocument);
    }
}

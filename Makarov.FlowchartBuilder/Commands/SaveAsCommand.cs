namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Saves the current document under a new file name.</summary>
public sealed class SaveAsCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.HasDocuments)
            Core.Instance.SaveDocument(Core.Instance.CurrentDocument, askForFileName: true);
    }
}

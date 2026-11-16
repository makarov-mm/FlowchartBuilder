namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Closes all documents, offering to save changes; stops when the user cancels.</summary>
public sealed class CloseAllCommand : Command
{
    public override void Run() => Core.Instance.CloseAllDocuments();
}

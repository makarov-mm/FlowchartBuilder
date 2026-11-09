namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Saves every document with unsaved changes.</summary>
public sealed class SaveAllCommand : Command
{
    public override void Run() => Core.Instance.SaveAllDocuments();
}

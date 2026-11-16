namespace Makarov.FlowchartBuilder.Commands;

public sealed class RedoCommand : Command
{
    public override void Run() => Core.Instance.Redo();
}

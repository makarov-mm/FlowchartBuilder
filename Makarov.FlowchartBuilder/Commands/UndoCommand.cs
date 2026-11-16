namespace Makarov.FlowchartBuilder.Commands;

public sealed class UndoCommand : Command
{
    public override void Run() => Core.Instance.Undo();
}

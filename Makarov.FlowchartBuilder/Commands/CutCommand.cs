namespace Makarov.FlowchartBuilder.Commands;

public sealed class CutCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { HasSelection: true } sheet)
            return;

        GetInstance<CopyCommand>().Run();
        GetInstance<DeleteCommand>().Run();
    }
}

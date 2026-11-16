namespace Makarov.FlowchartBuilder.Commands;

public sealed class CutCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { HasSelection: true } sheet)
            return;

        if (GlyphClipboard.Copy()) GetInstance<DeleteCommand>().Run();
    }
}

namespace Makarov.FlowchartBuilder.Commands;

public sealed class CopyCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is { HasSelection: true } sheet && GlyphClipboard.Copy())
            sheet.UpdateGlyphsCommands();
    }
}

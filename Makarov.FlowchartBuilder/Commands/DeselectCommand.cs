namespace Makarov.FlowchartBuilder.Commands;

public sealed class DeselectCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        sheet.UnlockGlyphs();
        sheet.DeselectAllGlyphs();
        sheet.UpdateGlyphsCommands();
        sheet.ShowProperties();
        Core.Instance.Redraw();
    }
}

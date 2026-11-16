namespace Makarov.FlowchartBuilder.Commands;

public sealed class DeleteCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { HasSelection: true } sheet)
            return;

        sheet.DeleteSelectedGlyphs();
        sheet.UpdateGlyphsCommands();
        sheet.ShowProperties();
        Core.Instance.Redraw();
    }
}

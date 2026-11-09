namespace Makarov.FlowchartBuilder.Commands;

public sealed class SelectAllCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        sheet.SelectAllGlyphs();
        sheet.UpdateGlyphsCommands();
        sheet.ShowProperties();
        Core.Instance.Redraw();
    }
}

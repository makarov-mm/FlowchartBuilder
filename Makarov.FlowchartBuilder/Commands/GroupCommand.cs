using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Joins the selected glyphs into one group, taking them out of the groups they were in.</summary>
public sealed class GroupCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        List<BlockGlyph> selected = [.. sheet.SelectedBlockGlyphs];
        if (selected.Count < 2)
            return;

        foreach (List<BlockGlyph> group in sheet.Groups)
            group.RemoveAll(selected.Contains);

        sheet.Groups.RemoveAll(group => group.Count < 2);
        sheet.Groups.Add(selected);

        sheet.UpdateGlyphsCommands();
        Core.Instance.Redraw();
    }
}

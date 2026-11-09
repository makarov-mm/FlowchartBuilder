using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Dissolves the groups of the selected glyphs.</summary>
public sealed class UngroupCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        foreach (List<BlockGlyph> group in sheet.SelectedBlockGlyphs.Select(glyph => glyph.Group).OfType<List<BlockGlyph>>().Distinct().ToList())
            sheet.Groups.Remove(group);

        sheet.UpdateGlyphsCommands();
        Core.Instance.Redraw();
    }
}

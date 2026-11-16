using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Unlocks all glyphs.</summary>
public sealed class UnfixAllCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        foreach (AbstractGlyph glyph in sheet.FixedGlyphs.ToList())
            glyph.Fixed = false;

        sheet.UpdateGlyphsCommands();
        sheet.ShowProperties();
        Core.Instance.Redraw();
    }
}

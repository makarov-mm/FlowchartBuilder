using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Locks all glyphs against selection and dragging.</summary>
public sealed class FixAllCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        foreach (AbstractGlyph glyph in sheet.Glyphs.ToList())
            glyph.Fixed = true;

        sheet.UpdateGlyphsCommands();
        sheet.ShowProperties();
        Core.Instance.Redraw();
    }
}

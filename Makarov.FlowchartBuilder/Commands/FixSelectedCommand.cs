using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Locks the selected glyphs against selection and dragging.</summary>
public sealed class FixSelectedCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        foreach (AbstractGlyph glyph in sheet.SelectedGlyphs.ToList())
            glyph.Fixed = true;

        sheet.UpdateGlyphsCommands();
        sheet.ShowProperties();
        Core.Instance.Redraw();
    }
}

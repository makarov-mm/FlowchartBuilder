using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Adds copies of the buffer to the current sheet; the buffer is kept, so it can be pasted again.</summary>
public sealed class PasteCommand : Command
{
    /// <summary>Each further paste of the same buffer is shifted by this distance, so copies do not hide each other.</summary>
    private const float RepeatOffsetInMM = 5f;

    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet || Core.Instance.Buffer.Count == 0)
            return;

        List<AbstractGlyph> pasted = GlyphCloner.Clone(Core.Instance.Buffer);

        sheet.DeselectAllGlyphs();
        for (int i = pasted.Count - 1; i >= 0; i--)
        {
            sheet.Add(pasted[i]);
            pasted[i].Selected = true;
        }

        foreach (BlockGlyph glyph in Core.Instance.Buffer.OfType<BlockGlyph>())
        {
            glyph.XInMM += RepeatOffsetInMM;
            glyph.YInMM += RepeatOffsetInMM;
        }

        sheet.UpdateGlyphsCommands();
        sheet.ShowProperties();
        Core.Instance.Redraw();
    }
}

using Makarov.FlowchartBuilder.Sheets;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>
/// Moves the selected glyphs together, keeping their layout, to an edge or the center of the paper.
/// </summary>
public abstract class AlignOnSheetCommand : Command
{
    public sealed override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        var glyphs = sheet.SelectedBlockGlyphs.ToList();
        if (glyphs.Count == 0)
            return;

        SizeF? paper = sheet is FixedSheet fixedSheet ? new SizeF(fixedSheet.Width, fixedSheet.Height) : null;
        if (GetOffset(AlignItem.GetBounds(glyphs), paper) is not { } offset)
            return;

        foreach (var glyph in glyphs)
        {
            glyph.XInMM += offset.X;
            glyph.YInMM += offset.Y;
        }

        Core.Instance.Redraw();
    }

    /// <summary>
    /// Offset in millimeters for the selection with the given bounds, or <see langword="null"/> when the alignment
    /// needs the paper size and the sheet has none.
    /// </summary>
    protected abstract PointF? GetOffset(RectangleF bounds, SizeF? paper);
}

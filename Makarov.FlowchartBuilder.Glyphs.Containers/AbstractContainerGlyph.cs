using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Containers;

/// <summary>
/// Container drawn as a faint tinted area; when selected it gets a dashed frame.
/// </summary>
[GlyphFamily("Containers")]
[GlyphDefaultWidth(60)]
[GlyphDefaultHeight(40)]
public abstract class AbstractContainerGlyph : ContainerGlyph
{
    private const int ThumbnailFrameInset = 5;

    private static readonly Brush AreaBrush = new SolidBrush(Color.FromArgb(5, Color.Blue));
    private static readonly Brush SelectedAreaBrush = new SolidBrush(Color.FromArgb(15, Color.Blue));
    private static readonly Pen SelectedFramePen = new(Color.Silver) { DashPattern = [4, 2] };
    private static readonly Brush ThumbnailFrameBrush = new SolidBrush(Color.FromArgb(128, Color.LightGray));

    /// <summary>Area of the palette image the container occupies.</summary>
    protected Rectangle ThumbnailFrame =>
        new(
            ThumbnailFrameInset,
            ThumbnailFrameInset,
            ThumbnailWidth - (ThumbnailFrameInset << 1),
            ThumbnailHeight - (ThumbnailFrameInset << 1));

    protected override void DrawGlyph(Graphics graphics) =>
        graphics.FillRectangle(AreaBrush, ScaledBounds);

    protected override void DrawSelectedGlyph(Graphics graphics)
    {
        Rectangle bounds = ScaledBounds;
        graphics.FillRectangle(SelectedAreaBrush, bounds);
        graphics.DrawRectangle(SelectedFramePen, bounds);
    }

    /// <summary>Fills the palette image frame; the border is left to the caller.</summary>
    protected static void FillThumbnailFrame(Graphics graphics, Rectangle frame) =>
        graphics.FillRectangle(ThumbnailFrameBrush, frame);

    /// <summary>Draws one slot of a stack or a table in the palette image.</summary>
    protected static void DrawThumbnailCell(Graphics graphics, Rectangle cell)
    {
        using var brush = new LinearGradientBrush(
            cell,
            Settings.Colors.ThumbnailGradientStart,
            Settings.Colors.ThumbnailObjectGradientEnd,
            90f);

        graphics.FillRectangle(brush, cell);
        graphics.DrawRectangle(Settings.Pens.ThumbnailBorder, cell);
    }
}

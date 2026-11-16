using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.Containers;

/// <summary>
/// Container drawn as a faint tinted area; when selected it gets a dashed frame. Containers are never rotated.
/// </summary>
[GlyphFamily("Containers")]
[GlyphDefaultWidth(60)]
[GlyphDefaultHeight(40)]
public abstract class AbstractContainerGlyph : ContainerGlyph
{
    private const int ThumbnailFrameInset = 5;

    private static readonly Color AreaColor = Color.FromArgb(5, Color.Blue);
    private static readonly Color SelectedAreaColor = Color.FromArgb(15, Color.Blue);
    private static readonly Stroke SelectedFrameStroke = new(Color.Silver) { Dash = new DashPattern(4f, 2f) };

    public override bool CanRotate => false;

    /// <summary>Area of the palette image the container occupies.</summary>
    protected Rectangle ThumbnailFrame =>
        new(
            ThumbnailFrameInset,
            ThumbnailFrameInset,
            ThumbnailWidth - (ThumbnailFrameInset << 1),
            ThumbnailHeight - (ThumbnailFrameInset << 1));

    protected override void DrawGlyph(Canvas canvas)
    {
        var bounds = ScaledBounds;
        float edge = Math.Max(1f, 2f * ScaleFactor);
        var shade = Color.FromArgb(40, Color.Black);
        canvas.FillRectangle(new RectangleF(bounds.Right, bounds.Top + edge, edge, bounds.Height), shade);
        canvas.FillRectangle(new RectangleF(bounds.Left + edge, bounds.Bottom, bounds.Width - edge, edge), shade);
        canvas.FillRectangle(bounds, AreaColor);
    }

    protected override void DrawSelectedGlyph(Canvas canvas)
    {
        Rectangle bounds = ScaledBounds;
        canvas.FillRectangle(bounds, SelectedAreaColor);
        canvas.DrawRectangle(bounds, SelectedFrameStroke);
    }

    /// <summary>Palette image background and the container frame filled; the frame border is drawn by the caller.</summary>
    protected void DrawThumbnailFrame(Canvas canvas)
    {
        base.DrawThumbnail(canvas);
        canvas.FillRectangle(ThumbnailFrame, new LinearGradient(ThumbnailFrame.Location,
            new PointF(ThumbnailFrame.Right, ThumbnailFrame.Bottom), Settings.Colors.ThumbnailObjectGradientStart, Settings.Colors.ThumbnailObjectGradientEnd));
    }

    /// <summary>Draws one slot of a stack or a table in the palette image.</summary>
    protected static void DrawThumbnailCell(Canvas canvas, Rectangle cell)
    {
        canvas.FillRectangle(
            cell,
            new LinearGradient(
                new PointF(cell.Left, cell.Top),
                new PointF(cell.Left, cell.Bottom),
                Settings.Colors.ThumbnailGradientStart,
                Settings.Colors.ThumbnailObjectGradientEnd));
        canvas.DrawRectangle(cell, Settings.Strokes.ThumbnailBorder);
    }
}

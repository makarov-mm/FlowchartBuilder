using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Flowchart;

/// <summary>Predefined process: a rectangle with double vertical sides.</summary>
[GlyphName("Subroutine")]
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(20)]
public sealed class SubroutineGlyph : AbstractFlowchartGlyph
{
    protected override Primitive CreatePrimitive(int x, int y, int width, int height) =>
        new Framework.Graphics.Primitives.Rectangle(x, y, width, height);

    protected override void DrawDetails(Canvas canvas, Stroke stroke)
    {
        int width = ScaledWidth;
        int top = ScaledY - ScaledHeight / 2;
        int bottom = ScaledY + ScaledHeight / 2;
        int left = ScaledX - width / 2 + width / 8;
        int right = ScaledX + width / 2 - width / 8;

        canvas.DrawLine(new PointF(left, top), new PointF(left, bottom), stroke);
        canvas.DrawLine(new PointF(right, top), new PointF(right, bottom), stroke);
    }

    protected override void DrawThumbnailDetails(Canvas canvas, RectangleF bounds)
    {
        float left = bounds.Left + bounds.Width / 8f;
        float right = bounds.Left + bounds.Width * 7f / 8f;

        canvas.DrawLine(new PointF(left, bounds.Top), new PointF(left, bounds.Bottom), Settings.Strokes.ThumbnailBorder);
        canvas.DrawLine(new PointF(right, bounds.Top), new PointF(right, bounds.Bottom), Settings.Strokes.ThumbnailBorder);
    }
}

using Makarov.FlowchartBuilder.API.Attributes;
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

    protected override void DrawDetails(Graphics graphics, Pen pen)
    {
        int width = ScaledWidth;
        int top = ScaledY - ScaledHeight / 2;
        int bottom = ScaledY + ScaledHeight / 2;
        int left = ScaledX - width / 2 + width / 8;
        int right = ScaledX + width / 2 - width / 8;

        graphics.DrawLine(pen, left, top, left, bottom);
        graphics.DrawLine(pen, right, top, right, bottom);
    }

    protected override void DrawThumbnailDetails(Graphics graphics, RectangleF bounds)
    {
        float left = bounds.Left + bounds.Width / 8f;
        float right = bounds.Left + bounds.Width * 7f / 8f;

        graphics.DrawLine(Settings.Pens.ThumbnailBorder, left, bounds.Top, left, bounds.Bottom);
        graphics.DrawLine(Settings.Pens.ThumbnailBorder, right, bounds.Top, right, bounds.Bottom);
    }
}

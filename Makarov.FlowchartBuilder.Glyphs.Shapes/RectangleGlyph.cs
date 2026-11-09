using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Shapes;

[GlyphName("Rectangle")]
public sealed class RectangleGlyph : AbstractShapeGlyph
{
    protected override int ThumbnailInsetY => ThumbnailHeight >> 2;

    protected override Primitive CreatePrimitive(int x, int y, int width, int height) =>
        new Framework.Graphics.Primitives.Rectangle(x, y, width, height);
}

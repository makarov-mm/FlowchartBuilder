using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Shapes;

[GlyphName("Ellipse")]
public sealed class EllypseGlyph : AbstractShapeGlyph
{
    protected override int ThumbnailInsetY => ThumbnailHeight >> 2;

    protected override Primitive CreatePrimitive(int x, int y, int width, int height) => new Circle(x, y, width, height);
}

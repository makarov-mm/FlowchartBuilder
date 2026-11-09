using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Shapes;

[GlyphName("Rounded Rectangle")]
public sealed class RoundedRectangleGlyph : AbstractShapeGlyph
{
    protected override int ThumbnailInsetY => ThumbnailHeight >> 2;

    protected override Primitive CreatePrimitive(int x, int y, int width, int height) => new RoundedRectangle(x, y, width, height, cornerDiameter: 15);
}

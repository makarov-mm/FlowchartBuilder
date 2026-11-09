using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Shapes;

[GlyphName("Triangle")]
public sealed class TriangleGlyph : AbstractShapeGlyph
{
    protected override Primitive CreatePrimitive(int x, int y, int width, int height) => new Triangle(x, y, width, height);
}

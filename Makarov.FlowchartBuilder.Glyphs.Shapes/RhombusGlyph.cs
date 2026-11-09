using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Shapes;

[GlyphName("Rhombus")]
public sealed class RhombusGlyph : AbstractShapeGlyph
{
    protected override Primitive CreatePrimitive(int x, int y, int width, int height) => new Rhombus(x, y, width, height);
}

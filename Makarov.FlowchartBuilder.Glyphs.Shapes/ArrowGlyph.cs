using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Shapes;

[GlyphName("Arrow")]
public sealed class ArrowGlyph : AbstractShapeGlyph
{
    protected override Primitive CreatePrimitive(int x, int y, int width, int height) => new Arrow(x, y, width, height);
}

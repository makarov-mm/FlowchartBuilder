using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Shapes;

[GlyphName("Cross")]
public sealed class CrossGlyph : AbstractShapeGlyph
{
    protected override Primitive CreatePrimitive(int x, int y, int width, int height) => new Cross(x, y, width, height);
}

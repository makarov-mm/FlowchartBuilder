using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Shapes;

[GlyphName("Pie")]
public sealed class PieGlyph : AbstractShapeGlyph
{
    protected override Primitive CreatePrimitive(int x, int y, int width, int height) => new Pie(x, y, width, height, startAngle: 180, sweepAngle: 180);
}

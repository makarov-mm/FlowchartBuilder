using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Flowchart;

/// <summary>Decision block.</summary>
[GlyphName("If")]
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(20)]
public sealed class IfGlyph : AbstractFlowchartGlyph
{
    protected override Primitive CreatePrimitive(int x, int y, int width, int height) => new Rhombus(x, y, width, height);
}

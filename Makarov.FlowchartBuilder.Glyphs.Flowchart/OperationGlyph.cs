using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Flowchart;

/// <summary>Processing step.</summary>
[GlyphName("Operation")]
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(20)]
public sealed class OperationGlyph : AbstractFlowchartGlyph
{
    protected override Primitive CreatePrimitive(int x, int y, int width, int height) =>
        new Framework.Graphics.Primitives.Rectangle(x, y, width, height);
}

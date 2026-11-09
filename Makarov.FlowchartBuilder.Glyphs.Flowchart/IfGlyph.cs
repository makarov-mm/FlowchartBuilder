using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Flowchart;

/// <summary>Decision block.</summary>
[GlyphName("If")]
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(20)]
public sealed class IfGlyph : AbstractFlowchartGlyph
{
    /// <summary>The largest rectangle inside the rhombus: half its width and height.</summary>
    protected override RectangleF TextRegion
    {
        get
        {
            RectangleF bounds = ScaledBounds;
            bounds.Inflate(-bounds.Width / 4f, -bounds.Height / 4f);
            return bounds;
        }
    }

    protected override Primitive CreatePrimitive(int x, int y, int width, int height) => new Rhombus(x, y, width, height);
}

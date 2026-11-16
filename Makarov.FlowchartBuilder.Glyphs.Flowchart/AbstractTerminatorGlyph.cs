using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Flowchart;

/// <summary>Start or end of a flowchart: a rectangle with fully rounded ends.</summary>
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(15)]
public abstract class AbstractTerminatorGlyph : AbstractFlowchartGlyph
{
    protected override int ThumbnailInsetY => ThumbnailHeight / 3;

    protected override Primitive CreatePrimitive(int x, int y, int width, int height) =>
        new RoundedRectangle(x, y, width, height, Math.Min(width, height));
}

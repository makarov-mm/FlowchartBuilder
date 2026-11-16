using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Flowchart;

/// <summary>Input or output block (parallelogram).</summary>
[GlyphName("In/Out")]
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(20)]
public sealed class InOutGlyph : AbstractFlowchartGlyph
{
    protected override Primitive CreatePrimitive(int x, int y, int width, int height) =>
        new BeveledRectangle(x, y, width, height, width >> 3);

    /// <summary>In the palette the bevel follows the whole image width, not the inset outline.</summary>
    protected override Geometry CreateThumbnailOutline()
    {
        int insetX = ThumbnailWidth >> 3;
        int insetY = ThumbnailInsetY;
        return new BeveledRectangle(
            insetX, insetY,
            ThumbnailWidth - (insetX << 1), ThumbnailHeight - (insetY << 1),
            ThumbnailWidth >> 3).Create();
    }
}

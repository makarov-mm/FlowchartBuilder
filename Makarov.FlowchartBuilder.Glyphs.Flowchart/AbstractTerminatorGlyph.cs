using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Flowchart;

/// <summary>
/// Start or end of a flowchart: a rectangle with rounded ends.
/// </summary>
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(15)]
public abstract class AbstractTerminatorGlyph : AbstractFlowchartGlyph
{
    protected override int ThumbnailInsetY => ThumbnailHeight / 3;

    protected override Primitive CreatePrimitive(int x, int y, int width, int height) =>
        new RoundedRectangle(x, y, width, height, Math.Min(width, height));

    /// <summary>On the sheet the corner diameter also follows the zoom.</summary>
    protected override GraphicsPath CreateOutline() =>
        new RoundedRectangle(
            ScaledX, ScaledY, ScaledWidth, ScaledHeight,
            (int)Math.Min(ScaledWidth * ScaleFactor, ScaledHeight * ScaleFactor)).CreateCentered();
}

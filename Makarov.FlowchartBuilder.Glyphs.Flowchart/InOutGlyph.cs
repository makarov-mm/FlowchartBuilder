using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Flowchart
{

    [GlyphName("In/Out")]
    [GlyphDefaultWidth(30)]
    [GlyphDefaultHeight(20)]
    public class InOutGlyph : AbstractFlowchartGlyph
    {

        protected override GraphicsPath CenteredPath
        {
            get
            {
                return new Framework.Graphics.Primitives.BeveledRectangle(
                    ScaledX, ScaledY,
                    ScaledWidth, ScaledHeight,
                    ScaledWidth >> 3).CreateCentered();
            }
        }

        protected override GraphicsPath Path
        {
            get
            {
                int offsetX = ThumbnailWidth >> 3;
                int offsetY = ThumbnailHeight >> 2;

                return new Framework.Graphics.Primitives.BeveledRectangle(
                    offsetX, offsetY,
                    ThumbnailWidth - (offsetX << 1), ThumbnailHeight - (offsetY << 1),
                    ThumbnailWidth >> 3).Create();
            }
        }
    }
}

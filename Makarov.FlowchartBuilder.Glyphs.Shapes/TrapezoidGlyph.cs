using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Shapes
{

    [GlyphName("Trapezoid")]
    public sealed class TrapezoidGlyph : AbstractShapeGlyph
    {

        protected override GraphicsPath CenteredPath
        {
            get
            {
                return new Framework.Graphics.Primitives.Trapezoid(
                    ScaledX, ScaledY,
                    ScaledWidth, ScaledHeight).CreateCentered();
            }
        }

        protected override GraphicsPath Path
        {
            get
            {
                int offsetX = ThumbnailWidth >> 3;
                int offsetY = ThumbnailHeight >> 3;

                return new Framework.Graphics.Primitives.Trapezoid(
                    offsetX, offsetY,
                    ThumbnailWidth - (offsetX << 1), ThumbnailHeight - (offsetY << 1)).Create();
            }
        }
    }
}

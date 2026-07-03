using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Shapes
{

    [GlyphName("Pie")]
    public sealed class PieGlyph : AbstractShapeGlyph
    {

        protected override GraphicsPath CenteredPath
        {
            get
            {
                return new Framework.Graphics.Primitives.Pie(
                    ScaledX, ScaledY,
                    ScaledWidth, ScaledHeight,
                    180, 180).CreateCentered();
            }
        }

        protected override GraphicsPath Path
        {
            get
            {
                int offsetX = ThumbnailWidth >> 3;
                int offsetY = ThumbnailHeight >> 3;

                return new Framework.Graphics.Primitives.Pie(
                    offsetX, offsetY,
                    ThumbnailWidth - (offsetX << 1), ThumbnailHeight - (offsetY << 1),
                    180, 180).Create();
            }
        }
    }
}

using System.Drawing;
using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Flowchart
{

    [GlyphName("Subroutine")]
    [GlyphDefaultWidth(30)]
    [GlyphDefaultHeight(20)]
    public class SubroutineGlyph : AbstractFlowchartGlyph
    {

        protected override GraphicsPath CenteredPath
        {
            get
            {
                return new Framework.Graphics.Primitives.Rectangle(
                    ScaledX, ScaledY,
                    ScaledWidth, ScaledHeight).CreateCentered();
            }
        }

        protected override GraphicsPath Path
        {
            get
            {
                int offsetX = ThumbnailWidth >> 3;
                int offsetY = ThumbnailHeight >> 2;

                return new Framework.Graphics.Primitives.Rectangle(
                    offsetX, offsetY,
                    ThumbnailWidth - (offsetX << 1), ThumbnailHeight - (offsetY << 1)).Create();
            }
        }

        protected override void DrawGlyph(Graphics gfx)
        {
            base.DrawGlyph(gfx);

            using (var pen = new Pen(BorderColor, ScaledBorderWidth))
            {
                gfx.DrawLine(
                    pen,
                    ScaledX - ScaledWidth / 2 + ScaledWidth / 8, ScaledY - ScaledHeight / 2,
                    ScaledX - ScaledWidth / 2 + ScaledWidth / 8, ScaledY + ScaledHeight / 2);

                gfx.DrawLine(
                    pen,
                    ScaledX + ScaledWidth / 2 - ScaledWidth / 8, ScaledY - ScaledHeight / 2,
                    ScaledX + ScaledWidth / 2 - ScaledWidth / 8, ScaledY + ScaledHeight / 2);
            }
        }

        public override Bitmap Thumbnail
        {
            get
            {
                Bitmap bmp = base.Thumbnail;

                using (GraphicsPath path = Path)
                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    gfx.DrawLine(
                        Settings.Pens.ThumbnailBorder,
                        path.GetBounds().Left + path.GetBounds().Width / 8f, path.GetBounds().Top,
                        path.GetBounds().Left + path.GetBounds().Width / 8f, path.GetBounds().Top + path.GetBounds().Height);

                    gfx.DrawLine(
                        Settings.Pens.ThumbnailBorder,
                        path.GetBounds().Left + path.GetBounds().Width * 7f / 8f, path.GetBounds().Top,
                        path.GetBounds().Left + path.GetBounds().Width * 7f / 8f, path.GetBounds().Top + path.GetBounds().Height);
                }

                return bmp;
            }
        }
    }
}

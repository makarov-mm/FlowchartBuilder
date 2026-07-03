using System.Drawing;
using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Shapes
{

    [GlyphFamily("Shapes")]
    [GlyphDefaultWidth(30)]
    [GlyphDefaultHeight(30)]
    public abstract class AbstractShapeGlyph : BlockGlyph
    {

        protected AbstractShapeGlyph()
        {
            BackgroundColor = Color.Yellow;
        }

        protected override void DrawText(Graphics gfx)
        {

        }

        protected abstract GraphicsPath CenteredPath
        {
            get;
        }

        protected abstract GraphicsPath Path
        {
            get;
        }

        protected override void DrawGlyph(Graphics gfx)
        {
            using (GraphicsPath path = CenteredPath)
            {
                using (Brush brush = new LinearGradientBrush(
                    new PointF(ScaledX - ScaledWidth / 2, ScaledY - ScaledHeight / 2),
                    new PointF(ScaledX + ScaledWidth / 2, ScaledY + ScaledHeight / 2),
                    BackgroundColor, BackgroundColor))
                {
                    gfx.FillPath(brush, path);
                }

                using (var pen = new Pen(BorderColor, ScaledBorderWidth))
                {
                    gfx.DrawPath(pen, path);
                }
            }
        }

        public override Bitmap Thumbnail
        {
            get
            {
                Bitmap bmp = base.Thumbnail;

                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    using (GraphicsPath path = Path)
                    {
                        using (Brush brush = new LinearGradientBrush(
                            new PointF(0, 0),
                            new PointF(ThumbnailWidth, ThumbnailHeight),
                            Settings.Colors.ThumbnailObjectGradientStart,
                            Settings.Colors.ThumbnailObjectGradientEnd))
                        {
                            gfx.FillPath(brush, path);
                        }

                        gfx.DrawPath(Settings.Pens.ThumbnailBorder, path);
                        DrawThumbnailBorder(gfx);
                    }
                }

                return bmp;
            }
        }
    }
}

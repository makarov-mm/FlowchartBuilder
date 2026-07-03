using System.Drawing;
using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Flowchart
{

    [GlyphFamily("Flowchart")]
    [GlyphDefaultWidth(30)]
    [GlyphDefaultHeight(30)]
    public abstract class AbstractFlowchartGlyph : BlockGlyph
    {

        protected AbstractFlowchartGlyph()
        {
            BackgroundColor = Color.Silver;
            ForegroundColor = Color.WhiteSmoke;
        }

        public override bool CanRotate
        {
            get { return false; }
        }

        public override bool SupportsLinks
        {
            get { return true; }
        }

        protected override void DrawLinkPoints(Graphics gfx)
        {
            float size = LinkPointSize;
            float half = size / 2f;

            using (var brush = new SolidBrush(Color.White))
            using (var pen = new Pen(Color.SteelBlue))
            {
                foreach (PointF point in LinkPointsScaled)
                {
                    gfx.FillEllipse(brush, point.X - half, point.Y - half, size, size);
                    gfx.DrawEllipse(pen, point.X - half, point.Y - half, size, size);
                }
            }
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
                    ForegroundColor, BackgroundColor))
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

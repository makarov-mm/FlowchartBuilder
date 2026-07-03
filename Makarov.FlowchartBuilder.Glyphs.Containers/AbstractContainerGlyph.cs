using System.Drawing;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Containers
{

    [GlyphFamily("Containers")]
    [GlyphDefaultWidth(60)]
    [GlyphDefaultHeight(40)]
    public abstract class AbstractContainerGlyph : ContainerGlyph
    {

        protected override void DrawGlyph(Graphics gfx)
        {
            using (Brush brush = new SolidBrush(Color.FromArgb(5, Color.Blue)))
            {
                gfx.FillRectangle(
                    brush,
                    ScaledX - (ScaledWidth >> 1), ScaledY - (ScaledHeight >> 1),
                    ScaledWidth, ScaledHeight);
            }
        }

        protected override void DrawSelectedGlyph(Graphics gfx)
        {
            using (Brush brush = new SolidBrush(Color.FromArgb(15, Color.Blue)))
            {
                gfx.FillRectangle(
                    brush,
                    ScaledX - (ScaledWidth >> 1), ScaledY - (ScaledHeight >> 1),
                    ScaledWidth, ScaledHeight);
            }

            using (var pen = new Pen(Color.Silver))
            {
                pen.DashPattern = new float[] { 4, 2 };
                gfx.DrawRectangle(
                    pen,
                    ScaledX - (ScaledWidth >> 1), ScaledY - (ScaledHeight >> 1),
                    ScaledWidth, ScaledHeight);
            }
        }
    }
}

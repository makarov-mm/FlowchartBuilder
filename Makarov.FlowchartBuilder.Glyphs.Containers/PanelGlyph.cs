using System.Drawing;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Containers
{

    [GlyphName("Panel")]
    public sealed class PanelGlyph : AbstractContainerGlyph
    {
        public override Bitmap Thumbnail
        {
            get
            {
                Bitmap bmp = base.Thumbnail;

                const int offset = 5;

                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(128, Color.LightGray)))
                        gfx.FillRectangle(
                            brush,
                            offset,
                            offset,
                            ThumbnailWidth - (offset << 1),
                            ThumbnailHeight - (offset << 1));

                    gfx.DrawRectangle(
                        Settings.Pens.ThumbnailBorder,
                        offset,
                        offset,
                        ThumbnailWidth - (offset << 1),
                        ThumbnailHeight - (offset << 1));

                }

                return bmp;
            }
        }
    }
}

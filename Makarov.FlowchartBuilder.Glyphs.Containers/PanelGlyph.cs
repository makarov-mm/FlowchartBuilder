using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Containers;

[GlyphName("Panel")]
public sealed class PanelGlyph : AbstractContainerGlyph
{
    public override Bitmap Thumbnail
    {
        get
        {
            Bitmap bitmap = base.Thumbnail;
            Rectangle frame = ThumbnailFrame;

            using Graphics graphics = Graphics.FromImage(bitmap);
            FillThumbnailFrame(graphics, frame);
            graphics.DrawRectangle(Settings.Pens.ThumbnailBorder, frame);

            return bitmap;
        }
    }
}

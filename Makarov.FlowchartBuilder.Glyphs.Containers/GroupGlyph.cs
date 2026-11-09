using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Containers;

[GlyphName("Group")]
public sealed class GroupGlyph : AbstractContainerGlyph
{
    private static readonly Pen ThumbnailFramePen = new(Settings.Colors.ThumbnailBorder) { DashStyle = DashStyle.Dash };

    public override Bitmap Thumbnail
    {
        get
        {
            Bitmap bitmap = base.Thumbnail;
            Rectangle frame = ThumbnailFrame;

            using Graphics graphics = Graphics.FromImage(bitmap);
            FillThumbnailFrame(graphics, frame);
            graphics.DrawRectangle(ThumbnailFramePen, frame);

            return bitmap;
        }
    }
}

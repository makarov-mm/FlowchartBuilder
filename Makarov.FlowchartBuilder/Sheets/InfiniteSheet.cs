using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.FlowchartBuilder.Extensions;

namespace Makarov.FlowchartBuilder.Sheets;

/// <summary>
/// Sheet without a paper size: it always fills the visible work area.
/// </summary>
[SheetFamily("Custom")]
[SheetName("Infinite")]
public sealed class InfiniteSheet : Sheet
{
    public override Bitmap Thumbnail
    {
        get
        {
            var bitmap = new Bitmap(ThumbnailWidth, ThumbnailHeight);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (var brush = new LinearGradientBrush(
                PointF.Empty, new PointF(bitmap.Width, bitmap.Height), Settings.Colors.SheetGradient, Color.White))
            {
                graphics.FillRectangle(brush, 0, 0, bitmap.Width, bitmap.Height);
            }

            bitmap.DrawBorder(Settings.Colors.ThumbnailBorder);
            return bitmap;
        }
    }

    public override Size GetPixelSize(bool scaled, Size viewport) => viewport;
}

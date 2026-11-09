using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Sheets;

/// <summary>
/// Sheet without a paper size: it always fills the visible work area.
/// </summary>
[SheetFamily("Custom")]
[SheetName("Infinite")]
public sealed class InfiniteSheet : Sheet
{
    protected override void DrawThumbnail(Canvas canvas)
    {
        canvas.FillRectangle(
            new RectangleF(0, 0, ThumbnailWidth, ThumbnailHeight),
            new LinearGradient(PointF.Empty, new PointF(ThumbnailWidth, ThumbnailHeight), Settings.Colors.SheetGradient, Color.White));
        DrawThumbnailBorder(canvas);
    }

    public override Size GetPixelSize(bool scaled, Size viewport) => viewport;
}

namespace Makarov.FlowchartBuilder.Extensions;

public static class BitmapExtensions
{
    /// <summary>Draws a one-pixel frame along the edges of the bitmap.</summary>
    public static void DrawBorder(this Bitmap bitmap, Color color)
    {
        using Graphics graphics = Graphics.FromImage(bitmap);
        using var pen = new Pen(color);
        graphics.DrawRectangle(pen, 0, 0, bitmap.Width - 1, bitmap.Height - 1);
    }
}

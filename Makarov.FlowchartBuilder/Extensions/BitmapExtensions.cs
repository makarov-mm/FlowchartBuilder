using System.Drawing;

namespace Makarov.FlowchartBuilder.Extensions
{

    public static class BitmapExtensions
    {

        public static void DrawBorder(this Bitmap bmp, Color color)
        {
            using (Graphics gfx = Graphics.FromImage(bmp))
            using (var pen = new Pen(color))
                gfx.DrawRectangle(pen, 0, 0, bmp.Width - 1, bmp.Height - 1);
        }
    }
}

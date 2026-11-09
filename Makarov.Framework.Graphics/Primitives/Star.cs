using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives;

/// <summary>Four-pointed star.</summary>
public sealed class Star(int x, int y, int width, int height) : Primitive(x, y, width, height)
{
    protected override GraphicsPath CreatePath(int left, int top, int width, int height)
    {
        int cx = left + width / 2;
        int cy = top + height / 2;
        int dx = width / 8;
        int dy = height / 8;

        return Polygon(
            new PointF(cx, top),
            new PointF(cx + dx, cy - dy),
            new PointF(left + width, cy),
            new PointF(cx + dx, cy + dy),
            new PointF(cx, top + height),
            new PointF(cx - dx, cy + dy),
            new PointF(left, cy),
            new PointF(cx - dx, cy - dy),
            new PointF(cx, top));
    }
}

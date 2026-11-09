using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives;

public sealed class Hourglass(int x, int y, int width, int height) : Primitive(x, y, width, height)
{
    protected override GraphicsPath CreatePath(int left, int top, int width, int height) => Polygon(
        new PointF(left, top),
        new PointF(left + width, top),
        new PointF(left, top + height),
        new PointF(left + width, top + height),
        new PointF(left, top));
}

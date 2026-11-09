using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives;

public sealed class Rhombus(int x, int y, int width, int height) : Primitive(x, y, width, height)
{
    protected override GraphicsPath CreatePath(int left, int top, int width, int height) => Polygon(
        new PointF(left + width / 2, top),
        new PointF(left + width, top + height / 2),
        new PointF(left + width / 2, top + height),
        new PointF(left, top + height / 2),
        new PointF(left + width / 2, top));
}

namespace Makarov.Framework.Graphics.Primitives;

public sealed class Rectangle(int x, int y, int width, int height) : Primitive(x, y, width, height)
{
    protected override Geometry CreateGeometry(int left, int top, int width, int height) => Polygon(
        new PointF(left, top),
        new PointF(left + width, top),
        new PointF(left + width, top + height),
        new PointF(left, top + height),
        new PointF(left, top));
}

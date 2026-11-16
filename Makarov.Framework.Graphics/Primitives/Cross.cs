namespace Makarov.Framework.Graphics.Primitives;

public sealed class Cross(int x, int y, int width, int height) : Primitive(x, y, width, height)
{
    protected override Geometry CreateGeometry(int left, int top, int width, int height) => Polygon(
        new PointF(left + width / 3, top),
        new PointF(left + width / 3 * 2, top),
        new PointF(left + width / 3 * 2, top + height / 3),
        new PointF(left + width, top + height / 3),
        new PointF(left + width, top + height / 3 * 2),
        new PointF(left + width / 3 * 2, top + height / 3 * 2),
        new PointF(left + width / 3 * 2, top + height),
        new PointF(left + width / 3, top + height),
        new PointF(left + width / 3, top + height / 3 * 2),
        new PointF(left, top + height / 3 * 2),
        new PointF(left, top + height / 3),
        new PointF(left + width / 3, top + height / 3),
        new PointF(left + width / 3, top));
}

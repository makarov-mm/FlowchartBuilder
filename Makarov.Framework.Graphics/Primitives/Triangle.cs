namespace Makarov.Framework.Graphics.Primitives;

/// <summary>Isosceles triangle pointing up.</summary>
public sealed class Triangle(int x, int y, int width, int height) : Primitive(x, y, width, height)
{
    protected override Geometry CreateGeometry(int left, int top, int width, int height) => Polygon(
        new PointF(left + width / 2, top),
        new PointF(left + width, top + height),
        new PointF(left, top + height),
        new PointF(left + width / 2, top));
}

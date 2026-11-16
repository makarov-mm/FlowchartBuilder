namespace Makarov.Framework.Graphics.Primitives;

/// <summary>Right triangle with the right angle in the bottom-left corner.</summary>
public sealed class DirectTriangle(int x, int y, int width, int height) : Primitive(x, y, width, height)
{
    protected override Geometry CreateGeometry(int left, int top, int width, int height) => Polygon(
        new PointF(left, top),
        new PointF(left + width, top + height),
        new PointF(left, top + height),
        new PointF(left, top));
}

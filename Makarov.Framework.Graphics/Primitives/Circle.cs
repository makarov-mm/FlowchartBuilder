namespace Makarov.Framework.Graphics.Primitives;

/// <summary>Ellipse inscribed into the bounding box.</summary>
public sealed class Circle(int x, int y, int width, int height) : Primitive(x, y, width, height)
{
    protected override Geometry CreateGeometry(int left, int top, int width, int height) =>
        Geometry.Ellipse(new RectangleF(left, top, width, height));
}

using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives;

/// <summary>Ellipse inscribed into the bounding box.</summary>
public sealed class Circle(int x, int y, int width, int height) : Primitive(x, y, width, height)
{
    protected override GraphicsPath CreatePath(int left, int top, int width, int height)
    {
        var path = new GraphicsPath();
        path.StartFigure();
        path.AddArc(left, top, width, height, 0, 360);
        path.CloseFigure();
        return path;
    }
}

using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives;

/// <summary>
/// A closed outline that fits into a width x height box. The caller owns the returned <see cref="GraphicsPath"/>.
/// </summary>
public abstract class Primitive
{
    protected Primitive(int x, int y, int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public int X { get; }

    public int Y { get; }

    public int Width { get; }

    public int Height { get; }

    /// <summary>Creates the outline with (<see cref="X"/>, <see cref="Y"/>) as the top-left corner.</summary>
    public GraphicsPath Create() => CreatePath(X, Y, Width, Height);

    /// <summary>Creates the outline with (<see cref="X"/>, <see cref="Y"/>) as the center.</summary>
    public GraphicsPath CreateCentered() => CreatePath(X - Width / 2, Y - Height / 2, Width, Height);

    protected abstract GraphicsPath CreatePath(int left, int top, int width, int height);

    protected static GraphicsPath Polygon(params PointF[] points)
    {
        var path = new GraphicsPath();
        path.StartFigure();
        path.AddLines(points);
        path.CloseFigure();
        return path;
    }
}

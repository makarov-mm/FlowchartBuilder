namespace Makarov.Framework.Graphics.Primitives;

/// <summary>
/// Immutable shape of a given size that produces its <see cref="Geometry"/> either from a top-left corner
/// (<see cref="Create"/>) or around a center point (<see cref="CreateCentered"/>).
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

    /// <summary>Outline with (<see cref="X"/>, <see cref="Y"/>) as the top-left corner.</summary>
    public Geometry Create() => CreateGeometry(X, Y, Width, Height);

    /// <summary>Outline with (<see cref="X"/>, <see cref="Y"/>) as the center.</summary>
    public Geometry CreateCentered() => CreateGeometry(X - Width / 2, Y - Height / 2, Width, Height);

    protected abstract Geometry CreateGeometry(int left, int top, int width, int height);

    /// <summary>Closed polygon; a last point equal to the first one is dropped.</summary>
    protected static Geometry Polygon(params ReadOnlySpan<PointF> points) =>
        points.Length > 1 && points[^1] == points[0] ? Geometry.Polygon(points[..^1]) : Geometry.Polygon(points);
}

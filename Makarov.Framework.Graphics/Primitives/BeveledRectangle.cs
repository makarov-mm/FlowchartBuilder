using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives;

/// <summary>Parallelogram whose top edge is shifted right by <see cref="Bevel"/>.</summary>
public sealed class BeveledRectangle(int x, int y, int width, int height, int bevel) : Primitive(x, y, width, height)
{
    public int Bevel { get; } = bevel;

    protected override GraphicsPath CreatePath(int left, int top, int width, int height) => Polygon(
        new PointF(left + Bevel, top),
        new PointF(left + width, top),
        new PointF(left + width - Bevel, top + height),
        new PointF(left, top + height),
        new PointF(left + Bevel, top));
}

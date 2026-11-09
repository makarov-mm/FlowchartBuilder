namespace Makarov.Framework.Graphics.Primitives;

public sealed class RoundedRectangle : Primitive
{
    public RoundedRectangle(int x, int y, int width, int height, float cornerDiameter)
        : base(x, y, width, height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cornerDiameter);
        CornerDiameter = cornerDiameter;
    }

    public float CornerDiameter { get; }

    protected override Geometry CreateGeometry(int left, int top, int width, int height)
    {
        float d = CornerDiameter;
        float r = d / 2f;

        return new GeometryBuilder()
            .BeginFigure(new PointF(left + r, top))
            .LineTo(new PointF(left + width - r, top))
            .Arc(new RectangleF(left + width - d, top, d, d), 270f, 90f)
            .LineTo(new PointF(left + width, top + height - r))
            .Arc(new RectangleF(left + width - d, top + height - d, d, d), 0f, 90f)
            .LineTo(new PointF(left + r, top + height))
            .Arc(new RectangleF(left, top + height - d, d, d), 90f, 90f)
            .LineTo(new PointF(left, top + r))
            .Arc(new RectangleF(left, top, d, d), 180f, 90f)
            .EndFigure(closed: true)
            .ToGeometry();
    }
}

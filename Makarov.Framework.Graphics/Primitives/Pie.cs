namespace Makarov.Framework.Graphics.Primitives;

/// <summary>Sector of the inscribed ellipse; angles are in degrees, clockwise from the X axis.</summary>
public sealed class Pie(int x, int y, int width, int height, int startAngle, int sweepAngle) : Primitive(x, y, width, height)
{
    public int StartAngle { get; } = startAngle;

    public int SweepAngle { get; } = sweepAngle;

    protected override Geometry CreateGeometry(int left, int top, int width, int height) =>
        new GeometryBuilder()
            .BeginFigure(new PointF(left + width / 2f, top + height / 2f))
            .Arc(new RectangleF(left, top, width, height), StartAngle, SweepAngle)
            .EndFigure(closed: true)
            .ToGeometry();
}

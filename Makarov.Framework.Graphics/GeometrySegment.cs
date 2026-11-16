namespace Makarov.Framework.Graphics;

/// <summary>
/// One segment of a figure, ending at <see cref="End"/>. Beziers use the two control points; arcs use the radius,
/// the sweep direction and the arc size, as in Direct2D.
/// </summary>
internal readonly record struct GeometrySegment(
    GeometrySegmentKind Kind,
    PointF End,
    PointF Control1 = default,
    PointF Control2 = default,
    SizeF Radius = default,
    bool Clockwise = true,
    bool LargeArc = false);

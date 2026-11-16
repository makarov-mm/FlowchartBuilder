using Makarov.Framework.Graphics.Interop;

namespace Makarov.Framework.Graphics;

/// <summary>
/// Immutable vector outline built with <see cref="GeometryBuilder"/>: the counterpart of a GDI+ path. The Direct2D
/// geometry is created on first use and released by <see cref="Dispose"/>.
/// </summary>
public sealed class Geometry : IDisposable
{
    private readonly GeometryFigure[] _figures;
    private nint _native;

    internal Geometry(GeometryFigure[] figures, RectangleF bounds)
    {
        _figures = figures;
        Bounds = bounds;
    }

    ~Geometry() => Com.Release(ref _native);

    /// <summary>Box around all points of the outline, including Bezier control points and whole arc ellipses.</summary>
    public RectangleF Bounds { get; }

    public static Geometry Polygon(params ReadOnlySpan<PointF> points) =>
        new GeometryBuilder().BeginFigure(points[0]).LinesTo(points[1..]).EndFigure(closed: true).ToGeometry();

    public static Geometry Ellipse(RectangleF bounds) =>
        new GeometryBuilder().Arc(bounds, 0f, 360f).EndFigure(closed: true).ToGeometry();

    public void Dispose()
    {
        Com.Release(ref _native);
        GC.SuppressFinalize(this);
    }

    internal nint GetNative()
    {
        if (_native == 0)
            _native = CreateNative();

        return _native;
    }

    private nint CreateNative()
    {
        nint geometry = D2DFactory.CreatePathGeometry();
        nint sink = 0;

        try
        {
            sink = D2DGeometry.Open(geometry);
            D2DGeometry.SetFillMode(sink, Native.D2D1_FILL_MODE_ALTERNATE);

            foreach (GeometryFigure figure in _figures)
            {
                D2DGeometry.BeginFigure(
                    sink, ToPoint(figure.Start), figure.Filled ? Native.D2D1_FIGURE_BEGIN_FILLED : Native.D2D1_FIGURE_BEGIN_HOLLOW);

                foreach (GeometrySegment segment in figure.Segments)
                    AddSegment(sink, segment);

                D2DGeometry.EndFigure(sink, figure.Closed ? Native.D2D1_FIGURE_END_CLOSED : Native.D2D1_FIGURE_END_OPEN);
            }

            D2DGeometry.Close(sink);
            return geometry;
        }
        catch
        {
            Com.Release(ref geometry);
            throw;
        }
        finally
        {
            Com.Release(ref sink);
        }
    }

    private static void AddSegment(nint sink, GeometrySegment segment)
    {
        switch (segment.Kind)
        {
            case GeometrySegmentKind.Line:
                D2DGeometry.AddLine(sink, ToPoint(segment.End));
                break;

            case GeometrySegmentKind.Bezier:
                D2DGeometry.AddBezier(sink, new Native.BezierSegment(ToPoint(segment.Control1), ToPoint(segment.Control2), ToPoint(segment.End)));
                break;

            case GeometrySegmentKind.Arc:
                D2DGeometry.AddArc(sink, new Native.ArcSegment
                {
                    Point = ToPoint(segment.End),
                    Size = new Native.Point2F(segment.Radius.Width, segment.Radius.Height),
                    RotationAngle = 0f,
                    SweepDirection = segment.Clockwise ? Native.D2D1_SWEEP_DIRECTION_CLOCKWISE : Native.D2D1_SWEEP_DIRECTION_COUNTER_CLOCKWISE,
                    ArcSize = segment.LargeArc ? Native.D2D1_ARC_SIZE_LARGE : Native.D2D1_ARC_SIZE_SMALL,
                });
                break;
        }
    }

    private static Native.Point2F ToPoint(PointF point) => new(point.X, point.Y);
}

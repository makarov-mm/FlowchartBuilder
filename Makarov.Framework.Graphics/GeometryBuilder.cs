namespace Makarov.Framework.Graphics;

/// <summary>Builds a <see cref="Geometry"/> figure by figure, with GDI+-compatible arcs.</summary>
public sealed class GeometryBuilder
{
    private readonly List<GeometryFigure> _figures = [];
    private GeometryFigure? _figure;
    private PointF _last;
    private float _left = float.MaxValue;
    private float _top = float.MaxValue;
    private float _right = float.MinValue;
    private float _bottom = float.MinValue;

    /// <summary>Starts a new figure, ending the current one (open) if it was not ended.</summary>
    public GeometryBuilder BeginFigure(PointF start, bool filled = true)
    {
        if (_figure is not null)
            EndFigure(closed: false);

        _figure = new GeometryFigure(start, filled);
        _figures.Add(_figure);
        _last = start;
        Include(start);
        return this;
    }

    public GeometryBuilder LineTo(PointF point)
    {
        Add(new GeometrySegment(GeometrySegmentKind.Line, point));
        return this;
    }

    public GeometryBuilder LinesTo(params ReadOnlySpan<PointF> points)
    {
        foreach (PointF point in points)
            LineTo(point);

        return this;
    }

    /// <summary>Cubic Bezier curve from the current point to <paramref name="end"/>.</summary>
    public GeometryBuilder BezierTo(PointF control1, PointF control2, PointF end)
    {
        Include(control1);
        Include(control2);
        Add(new GeometrySegment(GeometrySegmentKind.Bezier, end, control1, control2));
        return this;
    }

    /// <summary>
    /// Elliptical arc of the ellipse inscribed in <paramref name="bounds"/>, like GraphicsPath.AddArc: angles are in
    /// degrees, clockwise from the x axis, and a line joins the current point to the start of the arc. Without a current
    /// figure the arc starts a new one.
    /// </summary>
    public GeometryBuilder Arc(RectangleF bounds, float startAngle, float sweepAngle)
    {
        float radiusX = bounds.Width / 2f;
        float radiusY = bounds.Height / 2f;
        var center = new PointF(bounds.X + radiusX, bounds.Y + radiusY);
        PointF start = PointOnEllipse(center, radiusX, radiusY, startAngle);

        if (_figure is null)
            BeginFigure(start);
        else if (start != _last)
            LineTo(start);

        float sweep = Math.Clamp(sweepAngle, -360f, 360f);
        if (sweep == 0f || radiusX <= 0f || radiusY <= 0f)
            return this;

        Include(bounds.Location);
        Include(new PointF(bounds.Right, bounds.Bottom));

        // Direct2D arcs are defined by their end points, so long arcs are split into unambiguous parts of at most 90 degrees.
        int parts = (int)MathF.Ceiling(MathF.Abs(sweep) / 90f);
        float step = sweep / parts;
        var radius = new SizeF(radiusX, radiusY);

        for (int i = 1; i <= parts; i++)
        {
            PointF end = PointOnEllipse(center, radiusX, radiusY, startAngle + step * i);
            Add(new GeometrySegment(GeometrySegmentKind.Arc, end, Radius: radius, Clockwise: sweep > 0f));
        }

        return this;
    }

    public GeometryBuilder EndFigure(bool closed = true)
    {
        if (_figure is not null)
        {
            _figure.Closed = closed;
            _figure = null;
        }

        return this;
    }

    public Geometry ToGeometry()
    {
        if (_figure is not null)
            EndFigure(closed: false);

        RectangleF bounds = _figures.Count == 0
            ? RectangleF.Empty
            : RectangleF.FromLTRB(_left, _top, _right, _bottom);

        return new Geometry([.. _figures], bounds);
    }

    /// <summary>
    /// The point of the ellipse on the ray at <paramref name="angle"/> degrees from the center, which is how GDI+
    /// interprets arc angles of non-circular ellipses.
    /// </summary>
    private static PointF PointOnEllipse(PointF center, float radiusX, float radiusY, float angle)
    {
        double radians = angle * Math.PI / 180.0;
        double cos = Math.Cos(radians);
        double sin = Math.Sin(radians);

        if (Math.Abs(cos) > 1e-5 && Math.Abs(sin) > 1e-5 && radiusX > 0f && radiusY > 0f)
        {
            double parametric = Math.Atan2(sin / radiusY, cos / radiusX);
            cos = Math.Cos(parametric);
            sin = Math.Sin(parametric);
        }

        return new PointF(center.X + (float)(radiusX * cos), center.Y + (float)(radiusY * sin));
    }

    private void Add(GeometrySegment segment)
    {
        if (_figure is null)
            throw new InvalidOperationException("Call BeginFigure before adding segments.");

        _figure.Segments.Add(segment);
        _last = segment.End;
        Include(segment.End);
    }

    private void Include(PointF point)
    {
        _left = MathF.Min(_left, point.X);
        _top = MathF.Min(_top, point.Y);
        _right = MathF.Max(_right, point.X);
        _bottom = MathF.Max(_bottom, point.Y);
    }
}

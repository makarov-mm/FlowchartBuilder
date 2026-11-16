using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs;

/// <summary>
/// The path of a connector in sheet pixels. Ports are given with their outward normals (unit vectors pointing away
/// from the block), so curves and orthogonal routes leave and enter the blocks at right angles to their sides.
/// </summary>
public sealed class ConnectorRoute
{
    private const int CurveSteps = 32;

    // A curve is stored as its four Bezier points; other routes as their polyline vertices.
    private readonly PointF[] _points;
    private readonly bool _curve;

    private ConnectorRoute(PointF[] points, bool curve)
    {
        _points = points;
        _curve = curve;
        Polyline = curve ? FlattenBezier(points) : points;
    }

    public PointF Start => _points[0];

    public PointF End => _points[^1];

    /// <summary>The route as straight segments (curves are flattened), for hit testing and labels.</summary>
    public IReadOnlyList<PointF> Polyline { get; }

    /// <summary>Unit vector of the direction in which the route arrives at <see cref="End"/>.</summary>
    public PointF EndDirection
    {
        get
        {
            for (int i = _points.Length - 2; i >= 0; i--)
            {
                PointF direction = Subtract(End, _points[i]);
                if (Length(direction) > 0.01f)
                    return Normalize(direction);
            }

            return new PointF(1f, 0f);
        }
    }

    /// <summary>The point halfway along the route.</summary>
    public PointF Midpoint
    {
        get
        {
            float remaining = TotalLength() / 2f;

            for (int i = 1; i < Polyline.Count; i++)
            {
                float length = Distance(Polyline[i - 1], Polyline[i]);
                if (length >= remaining && length > 0f)
                    return Lerp(Polyline[i - 1], Polyline[i], remaining / length);

                remaining -= length;
            }

            return End;
        }
    }

    /// <param name="startBounds">Bounds of the source block, used to route orthogonal lines around it.</param>
    /// <param name="endBounds">Bounds of the target block.</param>
    /// <param name="spacing">Minimum distance a route keeps going straight out of a port, in pixels.</param>
    public static ConnectorRoute Create(
        ConnectorRouting routing,
        PointF start,
        PointF startNormal,
        RectangleF startBounds,
        PointF end,
        PointF endNormal,
        RectangleF endBounds,
        float spacing) => routing switch
        {
            ConnectorRouting.Straight => new ConnectorRoute([start, end], curve: false),
            ConnectorRouting.Orthogonal => new ConnectorRoute(
                RouteOrthogonal(start, startNormal, startBounds, end, endNormal, endBounds, spacing), curve: false),
            _ => new ConnectorRoute(RouteCurve(start, startNormal, end, endNormal, spacing), curve: true),
        };

    /// <summary>
    /// The route as a stroke-only geometry, shortened at the end by <paramref name="endTrim"/> pixels to leave room
    /// for an arrowhead.
    /// </summary>
    public Geometry CreateGeometry(float endTrim = 0f)
    {
        var builder = new GeometryBuilder().BeginFigure(Start, filled: false);
        PointF direction = EndDirection;

        if (_curve)
        {
            float trim = Math.Min(endTrim, Distance(_points[2], End) * 0.8f);
            PointF shift = Multiply(direction, trim);
            builder.BezierTo(_points[1], Subtract(_points[2], shift), Subtract(End, shift));
        }
        else
        {
            for (int i = 1; i < _points.Length - 1; i++)
                builder.LineTo(_points[i]);

            float trim = Math.Min(endTrim, Distance(_points[^2], End) * 0.8f);
            builder.LineTo(Subtract(End, Multiply(direction, trim)));
        }

        return builder.EndFigure(closed: false).ToGeometry();
    }

    /// <summary>Filled triangle with its tip at <see cref="End"/>.</summary>
    public Geometry CreateArrowhead(float length, float width)
    {
        PointF direction = EndDirection;
        var normal = new PointF(-direction.Y, direction.X);
        PointF basePoint = Subtract(End, Multiply(direction, length));
        PointF side = Multiply(normal, width / 2f);

        return Geometry.Polygon(End, Add(basePoint, side), Subtract(basePoint, side));
    }

    /// <summary>Distance from <paramref name="point"/> to the nearest point of the route.</summary>
    public float DistanceTo(PointF point)
    {
        float nearest = float.MaxValue;

        for (int i = 1; i < Polyline.Count; i++)
            nearest = Math.Min(nearest, DistanceToSegment(point, Polyline[i - 1], Polyline[i]));

        return nearest;
    }

    private static PointF[] RouteCurve(PointF start, PointF startNormal, PointF end, PointF endNormal, float spacing)
    {
        // The control points pull the curve straight out of the ports; the pull grows with the distance so that
        // long connectors stay smooth and short ones do not loop.
        float pull = Math.Max(Distance(start, end) * 0.4f, spacing);
        return [start, Add(start, Multiply(startNormal, pull)), Add(end, Multiply(endNormal, pull)), end];
    }

    private static PointF[] RouteOrthogonal(
        PointF start, PointF startNormal, RectangleF startBounds, PointF end, PointF endNormal, RectangleF endBounds, float spacing)
    {
        // Leave both ports along their normals first, then join the two stubs with horizontal and vertical segments.
        // A rotated terminal still leaves an orthogonal wire along its nearest cardinal axis.
        static PointF Cardinal(PointF n) => MathF.Abs(n.X) >= MathF.Abs(n.Y)
            ? new PointF(Math.Sign(n.X), 0) : new PointF(0, Math.Sign(n.Y));
        startNormal = Cardinal(startNormal);
        endNormal = Cardinal(endNormal);
        PointF a = Add(start, Multiply(startNormal, spacing));
        PointF b = Add(end, Multiply(endNormal, spacing));
        bool startHorizontal = startNormal.X != 0f;
        bool endHorizontal = endNormal.X != 0f;
        var points = new List<PointF> { start, a };

        if (startHorizontal && endHorizontal)
        {
            float middle = (a.X + b.X) / 2f;
            if (IsAhead(middle - a.X, startNormal.X) && IsAhead(middle - b.X, endNormal.X))
            {
                points.Add(new PointF(middle, a.Y));
                points.Add(new PointF(middle, b.Y));
            }
            else
            {
                // The ports face away from each other: go around below both blocks.
                float y = Math.Max(startBounds.Bottom, endBounds.Bottom) + spacing;
                points.Add(new PointF(a.X, y));
                points.Add(new PointF(b.X, y));
            }
        }
        else if (!startHorizontal && !endHorizontal)
        {
            float middle = (a.Y + b.Y) / 2f;
            if (IsAhead(middle - a.Y, startNormal.Y) && IsAhead(middle - b.Y, endNormal.Y))
            {
                points.Add(new PointF(a.X, middle));
                points.Add(new PointF(b.X, middle));
            }
            else
            {
                // The ports face away from each other: go around on the right of both blocks.
                float x = Math.Max(startBounds.Right, endBounds.Right) + spacing;
                points.Add(new PointF(x, a.Y));
                points.Add(new PointF(x, b.Y));
            }
        }
        else if (startHorizontal)
        {
            var corner = new PointF(b.X, a.Y);
            points.Add(IsAhead(corner.X - a.X, startNormal.X) && IsAhead(corner.Y - b.Y, endNormal.Y) ? corner : new PointF(a.X, b.Y));
        }
        else
        {
            var corner = new PointF(a.X, b.Y);
            points.Add(IsAhead(corner.Y - a.Y, startNormal.Y) && IsAhead(corner.X - b.X, endNormal.X) ? corner : new PointF(b.X, a.Y));
        }

        points.Add(b);
        points.Add(end);
        return Simplify(points);
    }

    /// <summary>Whether moving by <paramref name="delta"/> keeps going in the direction of <paramref name="normal"/>.</summary>
    private static bool IsAhead(float delta, float normal) => delta * normal >= 0f;

    /// <summary>Drops repeated points and points in the middle of a straight run.</summary>
    private static PointF[] Simplify(List<PointF> points)
    {
        var result = new List<PointF>(points.Count);

        foreach (PointF point in points)
        {
            if (result.Count > 0 && Distance(result[^1], point) < 0.01f)
                continue;

            if (result.Count >= 2 && IsCollinear(result[^2], result[^1], point))
                result[^1] = point;
            else
                result.Add(point);
        }

        return result.Count >= 2 ? [.. result] : [points[0], points[^1]];
    }

    private static bool IsCollinear(PointF a, PointF b, PointF c)
    {
        // Only runs in the same direction are merged; a route that turns back keeps its corner.
        float cross = (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);
        float dot = (b.X - a.X) * (c.X - b.X) + (b.Y - a.Y) * (c.Y - b.Y);
        return Math.Abs(cross) < 0.01f && dot > 0f;
    }

    private static PointF[] FlattenBezier(PointF[] bezier)
    {
        var points = new PointF[CurveSteps + 1];

        for (int i = 0; i <= CurveSteps; i++)
        {
            float t = (float)i / CurveSteps;
            float u = 1f - t;
            float a = u * u * u;
            float b = 3f * u * u * t;
            float c = 3f * u * t * t;
            float d = t * t * t;

            points[i] = new PointF(
                a * bezier[0].X + b * bezier[1].X + c * bezier[2].X + d * bezier[3].X,
                a * bezier[0].Y + b * bezier[1].Y + c * bezier[2].Y + d * bezier[3].Y);
        }

        return points;
    }

    private float TotalLength()
    {
        float length = 0f;
        for (int i = 1; i < Polyline.Count; i++)
            length += Distance(Polyline[i - 1], Polyline[i]);

        return length;
    }

    private static float DistanceToSegment(PointF point, PointF start, PointF end)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float lengthSquared = dx * dx + dy * dy;

        float t = lengthSquared == 0f
            ? 0f
            : Math.Clamp(((point.X - start.X) * dx + (point.Y - start.Y) * dy) / lengthSquared, 0f, 1f);

        return Distance(point, new PointF(start.X + t * dx, start.Y + t * dy));
    }

    private static PointF Add(PointF a, PointF b) => new(a.X + b.X, a.Y + b.Y);

    private static PointF Subtract(PointF a, PointF b) => new(a.X - b.X, a.Y - b.Y);

    private static PointF Multiply(PointF a, float factor) => new(a.X * factor, a.Y * factor);

    private static PointF Lerp(PointF a, PointF b, float t) => new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

    private static float Length(PointF a) => MathF.Sqrt(a.X * a.X + a.Y * a.Y);

    private static float Distance(PointF a, PointF b) => Length(Subtract(a, b));

    private static PointF Normalize(PointF a)
    {
        float length = Length(a);
        return length == 0f ? a : new PointF(a.X / length, a.Y / length);
    }
}

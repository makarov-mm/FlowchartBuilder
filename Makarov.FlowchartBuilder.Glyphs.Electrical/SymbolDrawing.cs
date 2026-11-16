using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.Electrical;

/// <summary>Maps the shared 40 × 24 symbol drawing area to canvas pixels, with a uniform pen width.</summary>
public sealed class SymbolDrawing(Canvas canvas, PointF center, float halfWidth, float halfHeight, Color color, float width)
{
    private readonly Stroke _pen = new(color, width) { Cap = StrokeCap.Round, Join = StrokeJoin.Round };
    private PointF Map(float x, float y) => new(center.X + x * halfWidth / 20, center.Y + y * halfHeight / 12);

    public void Line(float x1, float y1, float x2, float y2) => canvas.DrawLine(Map(x1, y1), Map(x2, y2), _pen);

    public void Leads(float left, float right)
    {
        Line(-20, 0, left, 0);
        Line(right, 0, 20, 0);
    }

    private RectangleF Box(float x, float y, float w, float h) => new(Map(x, y), new SizeF(w * halfWidth / 20, h * halfHeight / 12));

    public void Rectangle(float x, float y, float w, float h) => canvas.DrawRectangle(Box(x, y, w, h), _pen);
    public void Ellipse(float x, float y, float w, float h) => canvas.DrawEllipse(Box(x, y, w, h), _pen);
    public void Dot(float rx, float ry) => canvas.FillEllipse(Box(-rx, -ry, 2 * rx, 2 * ry), color);

    public void Triangle(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        using var geometry = new GeometryBuilder().BeginFigure(Map(x1, y1))
            .LineTo(Map(x2, y2)).LineTo(Map(x3, y3)).EndFigure().ToGeometry();
        canvas.FillGeometry(geometry, color);
    }

    public void Curve(float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2)
    {
        using var geometry = new GeometryBuilder().BeginFigure(Map(x1, y1), filled: false)
            .BezierTo(Map(cx1, cy1), Map(cx2, cy2), Map(x2, y2)).EndFigure(closed: false).ToGeometry();
        canvas.DrawGeometry(geometry, _pen);
    }
}

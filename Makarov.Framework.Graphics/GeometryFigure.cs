namespace Makarov.Framework.Graphics;

/// <summary>A connected sequence of segments; filled figures are painted by fills, hollow ones only by strokes.</summary>
internal sealed class GeometryFigure(PointF start, bool filled)
{
    public PointF Start { get; } = start;

    public bool Filled { get; } = filled;

    public bool Closed { get; set; }

    public List<GeometrySegment> Segments { get; } = [];
}

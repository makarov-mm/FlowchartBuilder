using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs;

/// <summary>
/// Arrow between the link ports of two block glyphs. Only the glyph ids and port numbers are saved;
/// <see cref="Relink"/> resolves them back to glyph instances after loading.
/// </summary>
public class ConnectorGlyph : AbstractGlyph
{
    private const int PortCount = 4;

    private static readonly Pen PreviewPen = CreateArrowPen(Color.DodgerBlue, 1.5f, DashStyle.Dash, LineCap.Flat);
    private static readonly Pen NormalPen = CreateArrowPen(Color.Black, 1.5f, DashStyle.Solid, LineCap.Round);
    private static readonly Pen SelectedPen = CreateArrowPen(Color.DodgerBlue, 2f, DashStyle.Solid, LineCap.Round);

    private BlockGlyph? _source;
    private BlockGlyph? _target;

    public ConnectorGlyph()
    {
    }

    public ConnectorGlyph(BlockGlyph source, int sourcePort, BlockGlyph target, int targetPort)
    {
        Source = source;
        Target = target;
        SourcePort = sourcePort;
        TargetPort = targetPort;
    }

    public BlockGlyph? Source
    {
        get => _source;
        set
        {
            _source = value;
            SourceGlyphId = value?.Id;
        }
    }

    public BlockGlyph? Target
    {
        get => _target;
        set
        {
            _target = value;
            TargetGlyphId = value?.Id;
        }
    }

    [ActiveProperty]
    public string? SourceGlyphId { get; set; }

    [ActiveProperty]
    public string? TargetGlyphId { get; set; }

    [ActiveProperty]
    public int SourcePort { get; set; }

    [ActiveProperty]
    public int TargetPort { get; set; }

    public bool IsResolved => _source is not null && _target is not null;

    /// <summary>Pen for the arrow being dragged out of a port before it is attached.</summary>
    public static Pen DragPreviewPen => PreviewPen;

    public bool Connects(AbstractGlyph glyph) => ReferenceEquals(glyph, _source) || ReferenceEquals(glyph, _target);

    /// <summary>Finds the source and target glyphs by their saved ids.</summary>
    public void Relink(IEnumerable<AbstractGlyph> glyphs)
    {
        _source = null;
        _target = null;

        foreach (BlockGlyph block in glyphs.OfType<BlockGlyph>())
        {
            if (_source is null && block.Id == SourceGlyphId)
                _source = block;

            if (_target is null && block.Id == TargetGlyphId)
                _target = block;
        }
    }

    public bool HitTest(PointF point, float tolerance) =>
        _source is not null && _target is not null &&
        DistanceToSegment(point, PortLocation(_source, SourcePort), PortLocation(_target, TargetPort)) <= tolerance;

    protected override void DrawGlyph(Graphics graphics)
    {
        if (_source is null || _target is null)
            return;

        graphics.DrawLine(Selected ? SelectedPen : NormalPen, PortLocation(_source, SourcePort), PortLocation(_target, TargetPort));
    }

    private static PointF PortLocation(BlockGlyph glyph, int port) =>
        glyph.LinkPointsScaled[Math.Clamp(port, 0, PortCount - 1)];

    private static float DistanceToSegment(PointF point, PointF start, PointF end)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float lengthSquared = dx * dx + dy * dy;

        float t = lengthSquared <= 0.0001f
            ? 0f
            : Math.Clamp(((point.X - start.X) * dx + (point.Y - start.Y) * dy) / lengthSquared, 0f, 1f);

        float ex = point.X - (start.X + t * dx);
        float ey = point.Y - (start.Y + t * dy);
        return MathF.Sqrt(ex * ex + ey * ey);
    }

    private static Pen CreateArrowPen(Color color, float width, DashStyle dashStyle, LineCap startCap)
    {
        using var arrowCap = new AdjustableArrowCap(4f, 5f, true);
        return new Pen(color, width)
        {
            StartCap = startCap,
            DashStyle = dashStyle,
            CustomEndCap = arrowCap,
        };
    }
}

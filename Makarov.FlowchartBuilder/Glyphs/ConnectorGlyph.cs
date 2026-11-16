using System.ComponentModel;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs;

/// <summary>
/// Line between link ports of two block glyphs. It follows the blocks when they move; its routing, arrowhead and
/// line can be changed in the Properties window, and its caption is shown in the middle of the line.
/// </summary>
public class ConnectorGlyph : AbstractGlyph
{
    /// <summary>Link ports of a block: [0] top, [1] right, [2] bottom, [3] left.</summary>
    public const int PortCount = 4;

    private const float SpacingInMM = 6f;
    private const float ArrowLengthInMM = 3f;
    private const float ArrowWidthRatio = 0.75f;
    private const float HitTolerance = 4f;
    private const float HandleSize = 6f;
    private const float CaptionPadding = 2f;

    private static readonly PointF[] PortNormals = [new(0f, -1f), new(1f, 0f), new(0f, 1f), new(-1f, 0f)];
    private static readonly TextStyle CaptionStyle = new("Arial", 8f);

    private BlockGlyph? _source;
    private BlockGlyph? _target;
    private float _lineWidth = 0.4f;

    public ConnectorGlyph()
    {
    }

    public ConnectorGlyph(BlockGlyph source, int sourcePort, BlockGlyph target, int targetPort)
    {
        Connect(source, sourcePort, target, targetPort);
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

    [Browsable(false)]
    [ActiveProperty]
    public string? SourceGlyphId { get; set; }

    [Browsable(false)]
    [ActiveProperty]
    public string? TargetGlyphId { get; set; }

    [Browsable(false)]
    [ActiveProperty]
    public int SourcePort { get; set; }

    [Browsable(false)]
    [ActiveProperty]
    public int TargetPort { get; set; }

    [Description("How the line runs between the blocks.")]
    [Category("Appearance")]
    [DisplayName("Routing")]
    [ActiveProperty]
    public ConnectorRouting Routing { get; set; } = ConnectorRouting.Curved;

    [Description("Arrowhead at the target end.")]
    [Category("Appearance")]
    [DisplayName("Arrow")]
    [ActiveProperty]
    public bool ArrowHead { get; set; } = true;

    [Description("Line color.")]
    [Category("Appearance")]
    [DisplayName("Line Color")]
    [ActiveProperty]
    public Color LineColor { get; set; } = Settings.Colors.Connector;

    [Description("Line width in mm.")]
    [Category("Appearance")]
    [DisplayName("Line Width")]
    [ActiveProperty]
    public float LineWidthInMM
    {
        get => _lineWidth;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            _lineWidth = value;
        }
    }

    private static float ScaleFactor => Core.Instance.CurrentDocument.DocumentSheet.ScaleFactor;

    /// <summary>Outward direction of a link port.</summary>
    public static PointF PortNormal(int port) => PortNormals[Math.Clamp(port, 0, PortCount - 1)];

    public static PointF PortLocation(BlockGlyph glyph, int port) => glyph.PortPosition(port, scaled: true);

    /// <summary>Route between two points with outward normals, at the zoom of the current document.</summary>
    public static ConnectorRoute CreateRoute(
        ConnectorRouting routing,
        PointF start,
        PointF startNormal,
        RectangleF startBounds,
        PointF end,
        PointF endNormal,
        RectangleF endBounds) =>
        ConnectorRoute.Create(
            routing,
            start,
            startNormal,
            startBounds,
            end,
            endNormal,
            endBounds,
            Math.Max(8f, UnitsConverter.MMToPxExact(SpacingInMM * ScaleFactor)));

    /// <summary>Route from a port of <paramref name="source"/> to any point, at the zoom of the current document.</summary>
    public static ConnectorRoute CreateRoute(
        ConnectorRouting routing, BlockGlyph source, int sourcePort, PointF end, PointF endNormal, RectangleF endBounds) =>
        CreateRoute(routing, PortLocation(source, sourcePort), source.PortDirection(sourcePort), source.RoutingBounds(true), end, endNormal, endBounds);

    /// <summary>Arrowhead length in pixels for a line of the given width, at the zoom of the current document.</summary>
    public static float ArrowLength(float lineWidth) =>
        Math.Max(Math.Max(6f, lineWidth * 4f), UnitsConverter.MMToPxExact(ArrowLengthInMM * ScaleFactor));

    /// <summary>Draws a route with an optional arrowhead; also used for the preview while a connector is dragged.</summary>
    public static void DrawRoute(Canvas canvas, ConnectorRoute route, Stroke stroke, bool arrowHead, float arrowLength)
    {
        using (Geometry line = route.CreateGeometry(arrowHead ? arrowLength * 0.9f : 0f))
            canvas.DrawGeometry(line, stroke);

        if (arrowHead)
        {
            using Geometry head = route.CreateArrowhead(arrowLength, arrowLength * ArrowWidthRatio);
            canvas.FillGeometry(head, stroke.Color);
        }
    }

    /// <summary>Square handles on both ends of a selected connector; they can be dragged to other ports.</summary>
    public static void DrawEndHandles(Canvas canvas, PointF start, PointF end)
    {
        foreach (PointF point in (ReadOnlySpan<PointF>)[start, end])
        {
            var handle = new RectangleF(point.X - HandleSize / 2f, point.Y - HandleSize / 2f, HandleSize, HandleSize);
            canvas.FillRectangle(handle, Settings.Colors.Selection);
            canvas.DrawRectangle(handle, Settings.Strokes.SelectionBorder);
        }
    }

    /// <summary>Line style of connectors at the current zoom.</summary>
    public static Stroke CreateStroke(Color color, float widthInMM) =>
        new(color, Math.Max(1f, UnitsConverter.MMToPxExact(widthInMM * ScaleFactor)))
        {
            Join = StrokeJoin.Round,
            Cap = StrokeCap.Round,
        };

    public void Connect(BlockGlyph source, int sourcePort, BlockGlyph target, int targetPort)
    {
        Source = source;
        Target = target;
        SourcePort = sourcePort;
        TargetPort = targetPort;
    }

    public bool Connects(AbstractGlyph glyph) => ReferenceEquals(glyph, _source) || ReferenceEquals(glyph, _target);

    /// <summary>Finds the source and target among <paramref name="glyphs"/> by their saved ids.</summary>
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

    /// <summary>The block attached at <paramref name="end"/>.</summary>
    public BlockGlyph? BlockAt(ConnectorEnd end) => end switch
    {
        ConnectorEnd.Source => _source,
        ConnectorEnd.Target => _target,
        _ => null,
    };

    /// <summary>The end whose handle is under <paramref name="point"/>; the arrowhead end wins when both are close.</summary>
    public ConnectorEnd HitEnd(PointF point)
    {
        if (_source is null || _target is null)
            return ConnectorEnd.None;

        if (IsNear(point, PortLocation(_target, TargetPort)))
            return ConnectorEnd.Target;

        return IsNear(point, PortLocation(_source, SourcePort)) ? ConnectorEnd.Source : ConnectorEnd.None;
    }

    /// <summary>Attaches <paramref name="end"/> to another port, of the same block or of another one.</summary>
    public void Reconnect(ConnectorEnd end, BlockGlyph block, int port)
    {
        switch (end)
        {
            case ConnectorEnd.Source:
                Source = block;
                SourcePort = port;
                break;

            case ConnectorEnd.Target:
                Target = block;
                TargetPort = port;
                break;
        }
    }

    public bool HitTest(PointF point) =>
        CreateRoute() is { } route && route.DistanceTo(point) <= HitTolerance + CreateStroke(LineColor, LineWidthInMM).Width / 2f;

    protected override void DrawGlyph(Canvas canvas)
    {
        if (CreateRoute() is { } route)
        {
            Stroke stroke = CreateStroke(LineColor, LineWidthInMM);
            DrawRoute(canvas, route, stroke, ArrowHead, ArrowLength(stroke.Width));
        }
    }

    protected override void DrawSelectedGlyph(Canvas canvas)
    {
        if (CreateRoute() is { } route)
        {
            Stroke stroke = CreateStroke(Settings.Colors.SelectedConnector, LineWidthInMM * 1.5f);
            DrawRoute(canvas, route, stroke, ArrowHead, ArrowLength(stroke.Width));
        }
    }

    protected override void DrawText(Canvas canvas)
    {
        if (string.IsNullOrWhiteSpace(Caption) || CreateRoute() is not { } route)
            return;

        TextStyle style = CaptionStyle.Scale(ScaleFactor);
        SizeF size = Canvas.MeasureText(Caption, style);
        PointF middle = route.Midpoint;
        var box = new RectangleF(
            middle.X - size.Width / 2f - CaptionPadding,
            middle.Y - size.Height / 2f - CaptionPadding,
            size.Width + CaptionPadding * 2f,
            size.Height + CaptionPadding * 2f);

        canvas.FillRectangle(box, Settings.Colors.ConnectorCaptionBackground);
        canvas.DrawText(Caption, style, box, LineColor, TextAlignment.Center, ParagraphAlignment.Center, wrap: false);
    }

    protected override void DrawDots(Canvas canvas)
    {
        if (_source is not null && _target is not null)
            DrawEndHandles(canvas, PortLocation(_source, SourcePort), PortLocation(_target, TargetPort));
    }

    protected override void DrawThumbnail(Canvas canvas)
    {
        base.DrawThumbnail(canvas);

        ConnectorRoute route = ConnectorRoute.Create(
            Routing,
            new PointF(7f, ThumbnailHeight - 12f),
            new PointF(1f, 0f),
            RectangleF.Empty,
            new PointF(ThumbnailWidth - 7f, 12f),
            new PointF(-1f, 0f),
            RectangleF.Empty,
            10f);

        var stroke = new Stroke(Settings.Colors.ThumbnailBorder, 1.5f) { Join = StrokeJoin.Round, Cap = StrokeCap.Round };
        DrawRoute(canvas, route, stroke, ArrowHead, arrowLength: 8f);
        DrawThumbnailBorder(canvas);
    }

    private static bool IsNear(PointF point, PointF handle)
    {
        const float reach = HandleSize / 2f + 2f;
        return Math.Abs(point.X - handle.X) <= reach && Math.Abs(point.Y - handle.Y) <= reach;
    }

    private ConnectorRoute? CreateRoute()
    {
        if (_source is null || _target is null)
            return null;

        return CreateRoute(
            Routing, _source, SourcePort, PortLocation(_target, TargetPort), _target.PortDirection(TargetPort), _target.RoutingBounds(true));
    }
}

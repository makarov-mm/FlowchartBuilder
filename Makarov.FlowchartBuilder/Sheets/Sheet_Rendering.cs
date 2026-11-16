using Makarov.FlowchartBuilder.Extensions;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Sheets;

public abstract partial class Sheet
{
    public void Render(Canvas canvas, Size size)
    {
        Point origin = ScaledViewOrigin;
        RenderBackground(canvas, size, origin, ScaleFactor, plain: false);
        using (canvas.Translate(-origin.X, -origin.Y))
        {
            canvas.Antialiasing = Settings.Environment.Antialiasing;
            RenderGlyphs(canvas, adornments: true);
            RenderInteraction(canvas);
        }
    }

    /// <summary>
    /// Renders the sheet at 100% zoom without selection or editing marks, <paramref name="resolution"/> times larger
    /// than on screen. For printing the background is plain white and the grid is left out.
    /// </summary>
    public Bitmap RenderImage(Size viewport, float resolution = 1f, bool forPrinting = false)
    {
        float scaleFactor = _scaleFactor;
        var selected = SelectedGlyphs.ToList();

        try
        {
            _scaleFactor = 1f;
            foreach (AbstractGlyph glyph in selected)
                glyph.Selected = false;

            Rectangle bounds = GetContentBounds();
            if (bounds.IsEmpty)
                bounds = new Rectangle(Point.Round(ViewOrigin), viewport);
            Size size = bounds.Size;
            int width = Math.Max(1, (int)MathF.Ceiling(size.Width * resolution));
            int height = Math.Max(1, (int)MathF.Ceiling(size.Height * resolution));

            return OffscreenRenderer.Render(width, height, canvas =>
            {
                using (canvas.Scale(resolution))
                {
                    RenderBackground(canvas, size, bounds.Location, 1f, plain: forPrinting);
                    canvas.Antialiasing = true;
                    using (canvas.Translate(-bounds.X, -bounds.Y))
                        RenderGlyphs(canvas, adornments: false);
                }
            });
        }
        finally
        {
            _scaleFactor = scaleFactor;
            foreach (AbstractGlyph glyph in selected)
                glyph.Selected = true;
        }
    }

    private static void RenderBackground(Canvas canvas, Size size, Point origin, float scale, bool plain)
    {
        canvas.FillRectangle(new RectangleF(0, 0, size.Width, size.Height), Settings.Colors.Sheet);
        if (plain || !Settings.Environment.Grid)
            return;

        float step = Settings.Environment.CurrentUnits.BigStep.CurrToPx() * scale;
        if (step <= 1f)
            return;

        // Grid lines are drawn aliased so that they stay one pixel wide and sharp.
        bool antialiasing = canvas.Antialiasing;
        canvas.Antialiasing = false;

        for (float x = -origin.X % step; x < size.Width; x += step)
            canvas.DrawLine(new PointF(MathF.Round(x), 0), new PointF(MathF.Round(x), size.Height), Settings.Strokes.SheetGrid);

        for (float y = -origin.Y % step; y < size.Height; y += step)
            canvas.DrawLine(new PointF(0, MathF.Round(y)), new PointF(size.Width, MathF.Round(y)), Settings.Strokes.SheetGrid);

        canvas.Antialiasing = antialiasing;
    }

    private void RenderGlyphs(Canvas canvas, bool adornments)
    {
        for (int i = _glyphs.Count - 1; i >= 0; i--)
        {
            // A connector whose end is being dragged is drawn by RenderInteraction in its new shape.
            if (!ReferenceEquals(_glyphs[i], _draggedConnector))
                _glyphs[i].Draw(canvas, adornments);
        }
    }

    private void RenderInteraction(Canvas canvas)
    {
        if (_selectionStart is { } start)
        {
            Rectangle selection = Normalize(start, _selectionEnd);
            canvas.FillRectangle(selection, Settings.Colors.Selection);
            canvas.DrawRectangle(selection, Settings.Strokes.SelectionBorder);
        }

        if (_connectorSource is { } source && _connectorCurrent is { } current && _connectorPrototype is { } prototype)
        {
            PointF from = ConnectorGlyph.PortLocation(source, _connectorSourcePort);
            var (end, endNormal, endBounds) = GetMovingEnd(current, towards: from);
            ConnectorRoute route = ConnectorGlyph.CreateRoute(prototype.Routing, source, _connectorSourcePort, end, endNormal, endBounds);

            Stroke stroke = ConnectorGlyph.CreateStroke(Settings.Colors.SelectedConnector, prototype.LineWidthInMM) with
            {
                Dash = new DashPattern(3f, 2f),
                Cap = StrokeCap.Flat,
            };

            ConnectorGlyph.DrawRoute(canvas, route, stroke, prototype.ArrowHead, ConnectorGlyph.ArrowLength(stroke.Width));
        }

        if (_draggedConnector is { Source: { } sourceBlock, Target: { } targetBlock } connector && _connectorCurrent is { } point)
            RenderDraggedConnector(canvas, connector, sourceBlock, targetBlock, point);
    }

    /// <summary>The connector whose end is being dragged, attached to the hovered port or following the mouse.</summary>
    private void RenderDraggedConnector(Canvas canvas, ConnectorGlyph connector, BlockGlyph source, BlockGlyph target, Point point)
    {
        PointF sourcePort = ConnectorGlyph.PortLocation(source, connector.SourcePort);
        PointF targetPort = ConnectorGlyph.PortLocation(target, connector.TargetPort);

        var (start, startNormal, startBounds) = _draggedEnd == ConnectorEnd.Source
            ? GetMovingEnd(point, towards: targetPort)
            : (sourcePort, ConnectorGlyph.PortNormal(connector.SourcePort), source.ScaledBounds);

        var (end, endNormal, endBounds) = _draggedEnd == ConnectorEnd.Target
            ? GetMovingEnd(point, towards: sourcePort)
            : (targetPort, ConnectorGlyph.PortNormal(connector.TargetPort), target.ScaledBounds);

        ConnectorRoute route = ConnectorGlyph.CreateRoute(connector.Routing, start, startNormal, startBounds, end, endNormal, endBounds);
        Stroke stroke = ConnectorGlyph.CreateStroke(Settings.Colors.SelectedConnector, connector.LineWidthInMM * 1.5f);

        ConnectorGlyph.DrawRoute(canvas, route, stroke, connector.ArrowHead, ConnectorGlyph.ArrowLength(stroke.Width));
        ConnectorGlyph.DrawEndHandles(canvas, route.Start, route.End);
    }

    /// <summary>
    /// The dragged end of a connector: the nearest port of the hovered block, or the mouse position, entered along the
    /// main axis of the direction <paramref name="towards"/> the other end.
    /// </summary>
    private (PointF Location, PointF Normal, RectangleF Bounds) GetMovingEnd(Point point, PointF towards)
    {
        if (_hoveredBlock is { } block)
            return (ConnectorGlyph.PortLocation(block, _hoveredPort), ConnectorGlyph.PortNormal(_hoveredPort), block.ScaledBounds);

        float dx = towards.X - point.X;
        float dy = towards.Y - point.Y;
        PointF normal = Math.Abs(dx) >= Math.Abs(dy) ? new PointF(Math.Sign(dx), 0f) : new PointF(0f, Math.Sign(dy));

        return (point, normal, new RectangleF(point, SizeF.Empty));
    }

    private static Rectangle Normalize(Point a, Point b) =>
        Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
}

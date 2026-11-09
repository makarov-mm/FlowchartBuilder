using Makarov.FlowchartBuilder.Extensions;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Sheets;

public abstract partial class Sheet
{
    private static readonly Color PrintBackground = Color.White;

    /// <summary>Size of the sheet in pixels, at the current zoom when <paramref name="scaled"/> is set.</summary>
    public abstract Size GetPixelSize(bool scaled, Size viewport);

    /// <summary>Renders the sheet with its origin at (0, 0) of the canvas.</summary>
    public void Render(Canvas canvas, Size size)
    {
        RenderBackground(canvas, size, scaled: true, plain: false);

        canvas.Antialiasing = Settings.Environment.Antialiasing;
        RenderGlyphs(canvas, adornments: true);
        RenderInteraction(canvas);
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

            Size size = GetPixelSize(scaled: false, viewport);
            int width = Math.Max(1, (int)MathF.Ceiling(size.Width * resolution));
            int height = Math.Max(1, (int)MathF.Ceiling(size.Height * resolution));

            return OffscreenRenderer.Render(width, height, canvas =>
            {
                using (canvas.Scale(resolution))
                {
                    RenderBackground(canvas, size, scaled: false, plain: forPrinting);
                    canvas.Antialiasing = true;
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

    /// <summary>Renders the image shown for this sheet format in the New Document dialog; the caller owns the bitmap.</summary>
    public Bitmap CreateThumbnail() => OffscreenRenderer.Render(ThumbnailWidth, ThumbnailHeight, DrawThumbnail);

    protected abstract void DrawThumbnail(Canvas canvas);

    protected static void DrawThumbnailBorder(Canvas canvas) =>
        canvas.DrawRectangle(new RectangleF(0, 0, ThumbnailWidth - 1, ThumbnailHeight - 1), Settings.Strokes.ThumbnailBorder);

    private static void RenderBackground(Canvas canvas, Size size, bool scaled, bool plain)
    {
        var area = new RectangleF(0, 0, size.Width, size.Height);

        if (plain)
        {
            canvas.FillRectangle(area, PrintBackground);
            return;
        }

        canvas.FillRectangle(area, new LinearGradient(PointF.Empty, new PointF(size.Width, size.Height), Settings.Colors.Sheet, Settings.Colors.SheetGradient));

        if (!Settings.Environment.Grid)
            return;

        int bigStep = Settings.Environment.CurrentUnits.BigStep.CurrToPx();
        float step = scaled ? bigStep.Scale() : bigStep;
        if (step <= 1f)
            return;

        // Grid lines are drawn aliased so that they stay one pixel wide and sharp.
        bool antialiasing = canvas.Antialiasing;
        canvas.Antialiasing = false;

        for (float x = step; x < size.Width; x += step)
            canvas.DrawLine(new PointF(MathF.Round(x), 0), new PointF(MathF.Round(x), size.Height), Settings.Strokes.SheetGrid);

        for (float y = step; y < size.Height; y += step)
            canvas.DrawLine(new PointF(0, MathF.Round(y)), new PointF(size.Width, MathF.Round(y)), Settings.Strokes.SheetGrid);

        canvas.Antialiasing = antialiasing;
    }

    private void RenderGlyphs(Canvas canvas, bool adornments)
    {
        for (int i = _glyphs.Count - 1; i >= 0; i--)
            _glyphs[i].Draw(canvas, adornments);
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
            ConnectorRoute route = CreateConnectorPreview(source, current, prototype.Routing);
            Stroke stroke = ConnectorGlyph.CreateStroke(Settings.Colors.SelectedConnector, prototype.LineWidthInMM) with
            {
                Dash = new DashPattern(3f, 2f),
                Cap = StrokeCap.Flat,
            };

            ConnectorGlyph.DrawRoute(canvas, route, stroke, prototype.ArrowHead, ConnectorGlyph.ArrowLength(stroke.Width));
        }
    }

    /// <summary>Route from the source port to the hovered port, or towards the mouse when no block is under it.</summary>
    private ConnectorRoute CreateConnectorPreview(BlockGlyph source, Point current, ConnectorRouting routing)
    {
        if (_connectorTarget is { } target)
        {
            return ConnectorGlyph.CreateRoute(
                routing,
                source,
                _connectorSourcePort,
                ConnectorGlyph.PortLocation(target, _connectorTargetPort),
                ConnectorGlyph.PortNormal(_connectorTargetPort),
                target.ScaledBounds);
        }

        // Arrive at the mouse along the main axis of the movement, as if it were a port facing the source.
        PointF from = ConnectorGlyph.PortLocation(source, _connectorSourcePort);
        float dx = from.X - current.X;
        float dy = from.Y - current.Y;
        PointF normal = Math.Abs(dx) >= Math.Abs(dy) ? new PointF(Math.Sign(dx), 0f) : new PointF(0f, Math.Sign(dy));

        return ConnectorGlyph.CreateRoute(routing, source, _connectorSourcePort, current, normal, new RectangleF(current, Size.Empty));
    }

    private static Rectangle Normalize(Point a, Point b) =>
        Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
}

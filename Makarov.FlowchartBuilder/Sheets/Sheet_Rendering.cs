using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Sheets;

public abstract partial class Sheet
{
    /// <summary>Size of the drawing surface in pixels: the paper at the current (or 100%) zoom, or the viewport for unbounded sheets.</summary>
    public abstract Size GetPixelSize(bool scaled, Size viewport);

    /// <summary>Paints the sheet at the current zoom with (0, 0) as its top-left corner.</summary>
    public void Render(Graphics graphics, Size size)
    {
        RenderBackground(graphics, size, scaled: true);
        RenderGlyphs(graphics);
        RenderInteraction(graphics);
    }

    /// <summary>Renders the sheet at 100% zoom without selection marks, for image export. The caller owns the bitmap.</summary>
    public Bitmap DrawUnscaled(Size viewport)
    {
        float scaleFactor = _scaleFactor;
        var selected = SelectedGlyphs.ToList();

        try
        {
            _scaleFactor = 1f;
            foreach (AbstractGlyph glyph in selected)
                glyph.Selected = false;

            Size size = GetPixelSize(scaled: false, viewport);
            var bitmap = new Bitmap(size.Width, size.Height);

            using Graphics graphics = Graphics.FromImage(bitmap);
            RenderBackground(graphics, size, scaled: false);
            RenderGlyphs(graphics);

            return bitmap;
        }
        finally
        {
            _scaleFactor = scaleFactor;
            foreach (AbstractGlyph glyph in selected)
                glyph.Selected = true;
        }
    }

    private static void RenderBackground(Graphics graphics, Size size, bool scaled)
    {
        using (var brush = new LinearGradientBrush(
            Point.Empty, new Point(size.Width, size.Height), Settings.Colors.Sheet, Settings.Colors.SheetGradient))
        {
            graphics.FillRectangle(brush, 0, 0, size.Width, size.Height);
        }

        if (!Settings.Environment.Grid)
            return;

        int bigStep = Settings.Environment.CurrentUnits.BigStep.CurrToPx();
        float step = scaled ? bigStep.Scale() : bigStep;
        if (step <= 0)
            return;

        Pen pen = Settings.Pens.SheetGrid;

        for (float x = step; x < size.Width; x += step)
            graphics.DrawLine(pen, x, 0, x, size.Height);

        for (float y = step; y < size.Height; y += step)
            graphics.DrawLine(pen, 0, y, size.Width, y);
    }

    /// <summary>Back to front, so that the front-most glyph is painted last.</summary>
    private void RenderGlyphs(Graphics graphics)
    {
        for (int i = _glyphs.Count - 1; i >= 0; i--)
            _glyphs[i].Draw(graphics);
    }

    /// <summary>Rubber-band selection and the connector being dragged.</summary>
    private void RenderInteraction(Graphics graphics)
    {
        if (_selectionStart is { } start)
        {
            Rectangle selection = Normalize(start, _selectionEnd);
            graphics.FillRectangle(Settings.Brushes.Selection, selection);
            graphics.DrawRectangle(Settings.Pens.SelectionBorder, selection);
        }

        if (_connectorSource is not null && _connectorCurrent is { } current)
        {
            PointF from = _connectorSource.LinkPointsScaled[_connectorSourcePort];
            graphics.DrawLine(ConnectorGlyph.DragPreviewPen, from, current);
        }
    }

    private static Rectangle Normalize(Point a, Point b) =>
        Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
}

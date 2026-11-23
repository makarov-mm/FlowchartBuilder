using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder;

public sealed partial class Core
{
    public event EventHandler? SelectionChanged;
    public event Action<Document>? DocumentClosed;

    internal void NotifySelectionChanged() => SelectionChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>Shows a glyph in its open document and selects it when its lock permits editing.</summary>
    public bool RevealGlyph(Document document, string glyphId)
    {
        var sheet = document.DocumentSheet;
        var glyph = sheet.Glyphs.FirstOrDefault(candidate => candidate.Id == glyphId);
        if (glyph is null || !_documents.Contains(document))
            return false;

        SelectDocument(document.Id);
        PointF? center = glyph switch
        {
            BlockGlyph block => new PointF(block.ScaledX, block.ScaledY),
            ConnectorGlyph connector => connector.CreateRoute()?.Midpoint,
            _ => null
        };
        if (center is { } point)
        {
            var view = MainWindow.SheetView;
            Rectangle viewport = view.RectangleToScreen(GetViewport(view.ClientSize));
            var screenPoint = new PointF(viewport.Left + point.X - sheet.ScaledViewOrigin.X,
                                         viewport.Top + point.Y - sheet.ScaledViewOrigin.Y);
            var floatingWindows = MainWindow.OwnedForms.Where(form => form.Visible && form.WindowState != FormWindowState.Minimized);
            if (FindRevealTarget(viewport, screenPoint, floatingWindows.Select(form => form.Bounds), glyph.Fixed) is { } target)
                sheet.ViewOrigin = new PointF((point.X - (target.X - viewport.Left)) / sheet.ScaleFactor,
                                              (point.Y - (target.Y - viewport.Top)) / sheet.ScaleFactor);
        }

        sheet.DeselectAllGlyphs();
        if (!glyph.Fixed)
            glyph.Selected = true;
        sheet.ShowProperties();
        UpdateCommands();
        Redraw();
        return true;
    }

    /// <summary>Returns an uncovered screen point to scroll to when the current point needs repositioning.</summary>
    internal static Point? FindRevealTarget(Rectangle viewport, PointF current, IEnumerable<Rectangle> windows, bool center = false)
    {
        if (viewport.Width <= 0 || viewport.Height <= 0)
            return null;

        var visible = new List<Rectangle> { viewport };
        foreach (var window in windows)
        {
            // Keep disjoint rectangles so overlapping tool windows cannot hide the chosen target.
            for (int i = visible.Count - 1; i >= 0; i--)
            {
                Rectangle area = visible[i];
                Rectangle blocked = Rectangle.Intersect(area, window);
                if (blocked.Width <= 0 || blocked.Height <= 0) continue;
                visible.RemoveAt(i);
                if (blocked.Left > area.Left)
                    visible.Add(Rectangle.FromLTRB(area.Left, area.Top, blocked.Left, area.Bottom));
                if (blocked.Right < area.Right)
                    visible.Add(Rectangle.FromLTRB(blocked.Right, area.Top, area.Right, area.Bottom));
                if (blocked.Top > area.Top)
                    visible.Add(Rectangle.FromLTRB(blocked.Left, area.Top, blocked.Right, blocked.Top));
                if (blocked.Bottom < area.Bottom)
                    visible.Add(Rectangle.FromLTRB(blocked.Left, blocked.Bottom, blocked.Right, area.Bottom));
            }
        }
        if (visible.Count == 0 || !center && visible.Any(area => ((RectangleF)area).Contains(current)))
            return null;

        Rectangle best = visible.MaxBy(area => (long)area.Width * area.Height);
        return new Point(best.Left + best.Width / 2, best.Top + best.Height / 2);
    }
}

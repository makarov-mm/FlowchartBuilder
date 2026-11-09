using System.Globalization;
using Makarov.FlowchartBuilder.Forms;
using Makarov.FlowchartBuilder.Sheets;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder;

public sealed partial class Core
{
    private const int SheetShadowSize = 3;

    /// <summary>How many pixels the current sheet overflows the visible work area horizontally and vertically.</summary>
    private static readonly TextStyle RulersTextStyle = new("Arial", 6f);

    private Size _scrollRange;

    public bool IsRedrawSuspended => _redrawSuspensions > 0;

    /// <summary>Suppresses <see cref="Redraw"/> until the returned scope is disposed. Scopes may be nested.</summary>
    public IDisposable SuspendRedraw()
    {
        _redrawSuspensions++;
        return new RedrawSuspension(this);
    }

    /// <summary>Updates the scroll bars and schedules a repaint of the work area.</summary>
    public void Redraw()
    {
        if (IsRedrawSuspended || _disposed || _mainWindow is not { IsDisposed: false } window)
            return;

        if (window.ClientSize.Width <= 0 || window.ClientSize.Height <= 0)
            return;

        UpdateScrollbars(window);
        window.SheetView.Invalidate();
    }

    /// <summary>Paints the work area of the main window: background, current sheet and rulers.</summary>
    /// <summary>Paints the sheet view: the work area, the sheet with its shadow and border, and the rulers.</summary>
    public void Paint(Canvas canvas)
    {
        canvas.Clear(Settings.Colors.WorkArea);

        Rectangle viewport = GetViewport(canvas.Size);
        Point origin = viewport.Location;

        if (ActiveSheet is { } sheet)
        {
            Size size = sheet.GetPixelSize(scaled: true, viewport.Size);
            origin = GetSheetOrigin(sheet, viewport);

            canvas.FillRectangle(
                new Rectangle(origin.X + size.Width, origin.Y + SheetShadowSize, SheetShadowSize, size.Height),
                Settings.Colors.SheetShadow);
            canvas.FillRectangle(
                new Rectangle(origin.X + SheetShadowSize, origin.Y + size.Height, size.Width - SheetShadowSize, SheetShadowSize),
                Settings.Colors.SheetShadow);

            using (canvas.Translate(origin.X, origin.Y))
            using (canvas.Clip(new Rectangle(Point.Empty, size)))
                sheet.Render(canvas, size);

            canvas.Antialiasing = true;
            canvas.DrawRectangle(new Rectangle(origin.X - 1, origin.Y - 1, size.Width + 1, size.Height + 1), Settings.Strokes.SheetBorder);
        }

        DrawRulers(canvas, viewport, origin);
    }

    /// <summary>Converts a point of the sheet view to sheet pixels at the current zoom.</summary>
    public Point ClientToSheet(Point viewPoint)
    {
        Rectangle viewport = GetViewport(MainWindow.SheetView.ClientSize);
        Point origin = ActiveSheet is { } sheet ? GetSheetOrigin(sheet, viewport) : viewport.Location;
        return new Point(viewPoint.X - origin.X, viewPoint.Y - origin.Y);
    }

    /// <summary>The part of the sheet view not covered by the rulers.</summary>
    private static Rectangle GetViewport(Size viewSize)
    {
        int rulers = Settings.Environment.Rulers ? Settings.Environment.RulersSize : 0;
        return new Rectangle(rulers, rulers, Math.Max(0, viewSize.Width - rulers), Math.Max(0, viewSize.Height - rulers));
    }

    private Point GetSheetOrigin(Sheet sheet, Rectangle inner)
    {
        if (sheet is not FixedSheet)
            return inner.Location;

        Size size = sheet.GetPixelSize(scaled: true, inner.Size);
        Point scroll = GetScrollOffset(MainWindow);

        return new Point(
            inner.Left + ((inner.Width - size.Width) >> 1) - scroll.X,
            inner.Top + ((inner.Height - size.Height) >> 1) - scroll.Y);
    }

    /// <summary>Scroll bars are centered at rest, so the offset runs from -range/2 to +range/2.</summary>
    private Point GetScrollOffset(MainForm window)
    {
        HScrollBar horizontal = window.HorizontalScrollBar;
        VScrollBar vertical = window.VerticalScrollBar;

        return new Point(
            horizontal.Visible ? horizontal.Value - (_scrollRange.Width >> 1) : 0,
            vertical.Visible ? vertical.Value - (_scrollRange.Height >> 1) : 0);
    }

    private void UpdateScrollbars(MainForm window)
    {
        HScrollBar horizontal = window.HorizontalScrollBar;
        VScrollBar vertical = window.VerticalScrollBar;
        bool wasHorizontalVisible = horizontal.Visible;
        bool wasVerticalVisible = vertical.Visible;

        int rulersBorder = Settings.Environment.Rulers ? (int)MathF.Ceiling(Settings.Strokes.RulersBorder.Width) : 0;
        Rectangle inner = window.InnerRectangle;
        var area = new Rectangle(
            inner.Left + rulersBorder,
            inner.Top + rulersBorder,
            inner.Width - rulersBorder,
            inner.Height - rulersBorder);

        Size range = Size.Empty;
        if (ActiveSheet is FixedSheet sheet)
        {
            Size size = sheet.GetPixelSize(scaled: true, area.Size);
            range = new Size(Math.Max(0, size.Width - area.Width), Math.Max(0, size.Height - area.Height));
        }

        horizontal.Visible = range.Width > 0;
        vertical.Visible = range.Height > 0;

        if (horizontal.Visible)
        {
            horizontal.SetBounds(
                area.Left, area.Bottom - horizontal.Height,
                area.Width - (vertical.Visible ? vertical.Width : 0), horizontal.Height);
            ConfigureScrollRange(horizontal, range.Width, _scrollRange.Width);
        }

        if (vertical.Visible)
        {
            vertical.SetBounds(
                area.Right - vertical.Width, area.Top,
                vertical.Width, area.Height - (horizontal.Visible ? horizontal.Height : 0));
            ConfigureScrollRange(vertical, range.Height, _scrollRange.Height);
        }

        _scrollRange = range;

        if (horizontal.Visible != wasHorizontalVisible || vertical.Visible != wasVerticalVisible)
        {
            foreach (DockingForm dockingWindow in Windows.Windows.OfType<DockingForm>())
                dockingWindow.UpdateDockedWindowPosition();
        }
    }

    /// <summary>
    /// The user can scroll up to Maximum - LargeChange + 1, so Maximum is padded to make the whole range reachable.
    /// A new range re-centers the view.
    /// </summary>
    private static void ConfigureScrollRange(ScrollBar scrollBar, int range, int previousRange)
    {
        if (range == previousRange)
            return;

        int step = Math.Max(1, range / 10);
        scrollBar.Minimum = 0;
        scrollBar.SmallChange = step;
        scrollBar.LargeChange = step;
        scrollBar.Maximum = range + step - 1;
        scrollBar.Value = range / 2;
    }

    private void DrawRulers(Canvas canvas, Rectangle viewport, Point origin)
    {
        if (!Settings.Environment.Rulers)
            return;

        const int size = Settings.Environment.RulersSize;
        int left = viewport.Left - size;
        int top = viewport.Top - size;

        Size view = canvas.Size;
        float scaleFactor = ActiveSheet?.ScaleFactor ?? 1f;
        Units units = Settings.Environment.CurrentUnits;
        Color background = Settings.Colors.RulersBackground;
        Color text = Settings.Colors.RulersText;
        Stroke border = Settings.Strokes.RulersBorder;
        Stroke tick = Settings.Strokes.RulersTick;

        canvas.FillRectangle(new Rectangle(left, top, viewport.Width + size, size), background);
        canvas.FillRectangle(new Rectangle(left, top, size, viewport.Height + size), background);
        canvas.DrawRectangle(new Rectangle(left, top, viewport.Width + size, size), border);
        canvas.DrawRectangle(new Rectangle(left, top, size, viewport.Height + size), border);

        for (int i = 0; UnitsConverter.CurrToPx(i) <= view.Width - origin.X; i += units.SmallStep)
        {
            int x = origin.X + UnitsConverter.CurrToPx(i);
            if (x < viewport.Left)
                continue;

            bool isMajor = i % units.BigStep == 0;
            if (isMajor)
                canvas.DrawText(FormatRulerLabel(i, scaleFactor), RulersTextStyle, new PointF(x + 1, top + (size >> 2)), text);

            canvas.DrawLine(new Point(x, top), new Point(x, top + (isMajor ? size >> 1 : size >> 2)), tick);
        }

        for (int i = 0; UnitsConverter.CurrToPx(i) <= view.Height - origin.Y; i += units.SmallStep)
        {
            int y = origin.Y + UnitsConverter.CurrToPx(i);
            if (y < viewport.Top)
                continue;

            bool isMajor = i % units.BigStep == 0;
            if (isMajor)
                canvas.DrawText(FormatRulerLabel(i, scaleFactor), RulersTextStyle, new PointF(left + (size >> 2), y + 1), text);

            canvas.DrawLine(new Point(left, y), new Point(left + (isMajor ? size >> 1 : size >> 2), y), tick);
        }

        canvas.FillRectangle(new Rectangle(left, top, size, size), background);
        canvas.DrawRectangle(new Rectangle(left, top, size, size), border);
        canvas.DrawText(units.ShortName, RulersTextStyle, new PointF(left + 1, top + 1), text);
    }

    private static string FormatRulerLabel(int value, float scaleFactor) =>
        (value / scaleFactor).ToString("F1", CultureInfo.CurrentCulture);

    private sealed class RedrawSuspension(Core core) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            core._redrawSuspensions--;
        }
    }
}

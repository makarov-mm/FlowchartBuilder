using System.Drawing.Drawing2D;
using System.Globalization;
using Makarov.FlowchartBuilder.Forms;
using Makarov.FlowchartBuilder.Sheets;
using Makarov.Framework.Core;

namespace Makarov.FlowchartBuilder;

public sealed partial class Core
{
    private const int SheetShadowSize = 3;

    /// <summary>How many pixels the current sheet overflows the visible work area horizontally and vertically.</summary>
    private Size _scrollRange;
    private Font? _rulersFont;

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
        window.Invalidate();
    }

    /// <summary>Paints the work area of the main window: background, current sheet and rulers.</summary>
    public void Paint(Graphics graphics)
    {
        graphics.Clear(Settings.Colors.WorkArea);

        Rectangle inner = MainWindow.InnerRectangle;
        Point origin = inner.Location;

        if (ActiveSheet is { } sheet)
        {
            Size size = sheet.GetPixelSize(scaled: true, inner.Size);
            origin = GetSheetOrigin(sheet, inner);

            graphics.FillRectangle(Brushes.Black, origin.X + size.Width, origin.Y + SheetShadowSize, SheetShadowSize, size.Height);
            graphics.FillRectangle(Brushes.Black, origin.X + SheetShadowSize, origin.Y + size.Height, size.Width - SheetShadowSize, SheetShadowSize);

            GraphicsState state = graphics.Save();
            graphics.TranslateTransform(origin.X, origin.Y);
            graphics.SetClip(new Rectangle(Point.Empty, size));
            sheet.Render(graphics, size);
            graphics.Restore(state);

            graphics.DrawRectangle(Settings.Pens.SheetBorder, origin.X - 1, origin.Y - 1, size.Width + 1, size.Height + 1);
        }

        DrawRulers(graphics, inner, origin);
    }

    /// <summary>Converts a point in main window client coordinates to sheet pixel coordinates.</summary>
    public Point ClientToSheet(Point clientPoint)
    {
        Rectangle inner = MainWindow.InnerRectangle;
        Point origin = ActiveSheet is { } sheet ? GetSheetOrigin(sheet, inner) : inner.Location;
        return new Point(clientPoint.X - origin.X, clientPoint.Y - origin.Y);
    }

    /// <summary>Fixed-size sheets are centered in the work area and scrolled; other sheets fill it.</summary>
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

        int rulersBorder = Settings.Environment.Rulers ? (int)MathF.Ceiling(Settings.Pens.RulersBorder.Width) : 0;
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

    private void DrawRulers(Graphics graphics, Rectangle inner, Point origin)
    {
        if (!Settings.Environment.Rulers)
            return;

        const int size = Settings.Environment.RulersSize;
        int left = inner.Left - size;
        int top = inner.Top - size;

        Size client = MainWindow.ClientSize;
        float scaleFactor = ActiveSheet?.ScaleFactor ?? 1f;
        Units units = Settings.Environment.CurrentUnits;
        Font font = _rulersFont ??= Fonts.Create("Arial", 6);
        Brush background = Settings.Brushes.RulersBackground;
        Pen border = Settings.Pens.RulersBorder;

        graphics.FillRectangle(background, left, top, inner.Width + size, size);
        graphics.FillRectangle(background, left, top, size, inner.Height + size);
        graphics.DrawRectangle(border, left, top, inner.Width + size, size);
        graphics.DrawRectangle(border, left, top, size, inner.Height + size);

        for (int i = 0; UnitsConverter.CurrToPx(i) <= client.Width - origin.X; i += units.SmallStep)
        {
            int x = origin.X + UnitsConverter.CurrToPx(i);
            bool isMajor = i % units.BigStep == 0;

            if (isMajor)
                graphics.DrawString(FormatRulerLabel(i, scaleFactor), font, Brushes.Black, x + 1, top + (size >> 2));

            graphics.DrawLine(Pens.Black, x, top, x, top + (isMajor ? size >> 1 : size >> 2));
        }

        for (int i = 0; UnitsConverter.CurrToPx(i) <= client.Height - origin.Y; i += units.SmallStep)
        {
            int y = origin.Y + UnitsConverter.CurrToPx(i);
            bool isMajor = i % units.BigStep == 0;

            if (isMajor)
                graphics.DrawString(FormatRulerLabel(i, scaleFactor), font, Brushes.Black, left + (size >> 2), y + 1);

            graphics.DrawLine(Pens.Black, left, y, left + (isMajor ? size >> 1 : size >> 2), y);
        }

        graphics.FillRectangle(background, left, top, size, size);
        graphics.DrawRectangle(border, left, top, size, size);
        graphics.DrawString(units.ShortName, font, Brushes.Black, left + 1, top + 1);
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

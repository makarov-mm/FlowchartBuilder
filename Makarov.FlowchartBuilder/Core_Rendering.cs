using System.Globalization;
using Makarov.FlowchartBuilder.Forms;
using Makarov.FlowchartBuilder.Sheets;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder;

public sealed partial class Core
{
    private static readonly TextStyle RulersTextStyle = new("Arial", 6f);

    public bool IsRedrawSuspended => _redrawSuspensions > 0;

    /// <summary>Suppresses <see cref="Redraw"/> until the returned scope is disposed. Scopes may be nested.</summary>
    public IDisposable SuspendRedraw()
    {
        _redrawSuspensions++;
        return new RedrawSuspension(this);
    }

    /// <summary>Schedules a repaint of the work area.</summary>
    public void Redraw()
    {
        if (IsRedrawSuspended || _disposed || _mainWindow is not { IsDisposed: false } window)
            return;

        if (window.ClientSize.Width <= 0 || window.ClientSize.Height <= 0)
            return;

        window.SheetView.Invalidate();
    }

    /// <summary>Paints the visible canvas and the rulers into the Direct2D view.</summary>
    public void Paint(Canvas canvas)
    {
        canvas.Clear(Settings.Colors.WorkArea);
        Rectangle viewport = GetViewport(canvas.Size);
        if (ActiveSheet is { } sheet && viewport.Width > 0 && viewport.Height > 0)
        {
            using (canvas.Clip(viewport))
            using (canvas.Translate(viewport.X, viewport.Y))
                sheet.Render(canvas, viewport.Size);
        }
        DrawRulers(canvas, viewport, ActiveSheet?.ScaledViewOrigin ?? Point.Empty);
    }

    public Point ClientToSheet(Point viewPoint)
    {
        Rectangle viewport = GetViewport(MainWindow.SheetView.ClientSize);
        Point offset = ActiveSheet?.ScaledViewOrigin ?? Point.Empty;
        return new Point(viewPoint.X - viewport.Left + offset.X, viewPoint.Y - viewport.Top + offset.Y);
    }

    /// <summary>The part of the sheet view not covered by the rulers.</summary>
    private static Rectangle GetViewport(Size viewSize)
    {
        int rulers = Settings.Environment.Rulers ? Settings.Environment.RulersSize : 0;
        return new Rectangle(rulers, rulers, Math.Max(0, viewSize.Width - rulers), Math.Max(0, viewSize.Height - rulers));
    }

    private void DrawRulers(Canvas canvas, Rectangle viewport, Point origin)
    {
        if (!Settings.Environment.Rulers)
            return;

        const int size = Settings.Environment.RulersSize;
        int left = viewport.Left - size;
        int top = viewport.Top - size;

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

        float step = UnitsConverter.CurrToPx(units.BigStep) * scaleFactor * units.SmallStep / units.BigStep;
        DrawTicks(true, origin.X, viewport.Width);
        DrawTicks(false, origin.Y, viewport.Height);

        void DrawTicks(bool horizontal, int offset, int length)
        {
            long first = (long)Math.Ceiling(offset / (double)step);
            for (long index = first; ; index++)
            {
                float position = (float)(index * (double)step - offset);
                if (position >= length) break;
                long value = index * units.SmallStep;
                bool major = value % units.BigStep == 0;
                int mark = major ? size / 2 : size / 4;
                float x = horizontal ? viewport.Left + position : left;
                float y = horizontal ? top : viewport.Top + position;
                canvas.DrawLine(new PointF(x, y), new PointF(horizontal ? x : x + mark, horizontal ? y + mark : y), tick);
                if (major)
                    canvas.DrawText(value.ToString(CultureInfo.CurrentCulture), RulersTextStyle,
                        new PointF(x + (horizontal ? 1 : size / 4), y + (horizontal ? size / 4 : 1)), text);
            }
        }

        canvas.FillRectangle(new Rectangle(left, top, size, size), background);
        canvas.DrawRectangle(new Rectangle(left, top, size, size), border);
        canvas.DrawText(units.ShortName, RulersTextStyle, new PointF(left + 1, top + 1), text);
    }

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

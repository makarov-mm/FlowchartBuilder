using System;
using System.Drawing;
using System.Windows.Forms;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.FlowchartBuilder.Forms;
using Makarov.FlowchartBuilder.Sheets;

namespace Makarov.FlowchartBuilder
{

    public sealed partial class Core
    {

        private Point UpdateScrollbars(MainForm wndMain)
        {

            HScrollBar hbar = wndMain.HorizontalScrollBar;
            VScrollBar vbar = wndMain.VerticalScrollBar;

            var rulersBorderWidth = (int)Math.Ceiling(
                Settings.Environment.Rulers
                    ? Settings.Pens.RulersBorder.Width
                    : 0);

            if (IsDocumentsExists && CurrentDocument.DocumentSheet is FixedSheet)
            {

                var sheet = (FixedSheet)CurrentDocument.DocumentSheet;

                var rect = wndMain.InnerRectangle;

                bool rulersVisibleChanged = false;

                rect = new Rectangle(
                    rect.Left + rulersBorderWidth,
                    rect.Top + rulersBorderWidth,
                    rect.Width - rulersBorderWidth,
                    rect.Height - rulersBorderWidth);

                if (sheet.ScaledWidth.MMToPx() > rect.Width)
                {
                    if (!hbar.Visible)
                    {
                        hbar.Visible = true;
                        rulersVisibleChanged = true;
                    }
                }
                else
                {
                    if (hbar.Visible)
                    {
                        hbar.Visible = false;
                        rulersVisibleChanged = true;
                    }
                }

                if (sheet.ScaledHeight.MMToPx() > rect.Height)
                {
                    if (!vbar.Visible)
                    {
                        vbar.Visible = true;
                        rulersVisibleChanged = true;
                    }

                }
                else
                {
                    if (vbar.Visible)
                    {
                        vbar.Visible = false;
                        rulersVisibleChanged = true;
                    }
                }

                if (hbar.Visible)
                {
                    hbar.Left = rect.Left;
                    hbar.Top = rect.Top + rect.Height - hbar.Height;
                    hbar.Width = rect.Width - (vbar.Visible ? vbar.Width : 0);

                    int dx = sheet.ScaledWidth.MMToPx() - rect.Width;

                    if (hbar.Maximum != dx)
                    {
                        hbar.Minimum = 0;
                        hbar.Maximum = dx;
                        hbar.SmallChange = dx / 10;
                        hbar.LargeChange = dx / 10;
                        hbar.Value = dx >> 1;
                    }
                }

                if (vbar.Visible)
                {
                    vbar.Left = rect.Left + rect.Width - vbar.Width;
                    vbar.Top = rect.Top;
                    vbar.Height = rect.Height - (hbar.Visible ? hbar.Height : 0);

                    int dy = sheet.ScaledHeight.MMToPx() - rect.Height;

                    if (vbar.Maximum != dy)
                    {
                        vbar.Minimum = 0;
                        vbar.Maximum = dy;
                        vbar.SmallChange = dy / 10;
                        vbar.LargeChange = dy / 10;
                        vbar.Value = dy;
                    }
                }

                if (rulersVisibleChanged)
                {

                    foreach (var wnd in Windows.Windows)
                        if (wnd is DockingForm)
                            ((DockingForm)wnd).UpdateDockedWindowPosition();
                }
            }

            return new Point(
                hbar.Visible ? hbar.Value - (hbar.Maximum >> 1) : 0,
                vbar.Visible ? vbar.Value - (vbar.Maximum >> 1) : 0);
        }

        private void DrawRulers(Graphics gfx, Rectangle clientRect,
            float leftOffset, float topOffset)
        {
            if (!Settings.Environment.Rulers) return;

            MainForm wndMain = MainWindow;

            float scaleFactor = IsDocumentsExists
                ? CurrentDocument.DocumentSheet.ScaleFactor
                : 1f;

            int rulersSize = Settings.Environment.RulersSize;

            using (Brush brush = new SolidBrush(Settings.Colors.RulersBackground))
            {
                gfx.FillRectangle(
                    brush,
                    clientRect.Left - rulersSize,
                    clientRect.Top - rulersSize,
                    clientRect.Width + rulersSize,
                    Settings.Environment.RulersSize
                    );

                gfx.FillRectangle(
                    brush,
                    clientRect.Left - rulersSize,
                    clientRect.Top - rulersSize,
                    Settings.Environment.RulersSize,
                    clientRect.Height + rulersSize
                    );
            }

            gfx.DrawRectangle(
                Settings.Pens.RulersBorder,
                clientRect.Left - rulersSize,
                clientRect.Top - rulersSize,
                clientRect.Width + rulersSize,
                Settings.Environment.RulersSize
                );

            gfx.DrawRectangle(
                Settings.Pens.RulersBorder,
                clientRect.Left - rulersSize,
                clientRect.Top - rulersSize,
                Settings.Environment.RulersSize,
                clientRect.Height + rulersSize
                );

            using (Font font = "Arial".SafeCreateFont(6))
            {
                Units units = Settings.Environment.CurrentUnits;

                for (int i = 0;
                     UnitsConverter.CurrToPx(i) <= wndMain.ClientSize.Width - leftOffset;
                     i += units.SmallStep)
                {
                    int len;
                    if (i % units.BigStep == 0)
                    {
                        len = rulersSize >> 1;
                        gfx.DrawString((i / scaleFactor).ToString("F1"), font, Brushes.Black,
                                        leftOffset + UnitsConverter.CurrToPx(i) + 1,
                                        clientRect.Top - rulersSize + (rulersSize >> 2));
                    }
                    else
                        len = rulersSize >> 2;

                    gfx.DrawLine(
                        Pens.Black,
                        leftOffset + UnitsConverter.CurrToPx(i), clientRect.Top - rulersSize,
                        leftOffset + UnitsConverter.CurrToPx(i), clientRect.Top - rulersSize + len);
                }

                for (int i = 0;
                     UnitsConverter.CurrToPx(i) <= wndMain.ClientSize.Height - topOffset;
                     i += units.SmallStep)
                {
                    int len;
                    if (i % units.BigStep == 0)
                    {
                        len = rulersSize >> 1;
                        gfx.DrawString((i / scaleFactor).ToString("F1"), font, Brushes.Black,
                                        clientRect.Left - rulersSize + (rulersSize >> 2),
                                        topOffset + UnitsConverter.CurrToPx(i) + 1);
                    }
                    else
                        len = rulersSize >> 2;

                    gfx.DrawLine(
                        Pens.Black,
                        clientRect.Left - rulersSize, topOffset + UnitsConverter.CurrToPx(i),
                        clientRect.Left - rulersSize + len, topOffset + UnitsConverter.CurrToPx(i));
                }

                using (Brush brush = new SolidBrush(Settings.Colors.RulersBackground))
                {
                    gfx.FillRectangle(
                        brush,
                        clientRect.Left - rulersSize,
                        clientRect.Top - rulersSize,
                        Settings.Environment.RulersSize,
                        Settings.Environment.RulersSize
                        );
                }

                gfx.DrawRectangle(
                    Settings.Pens.RulersBorder,
                    clientRect.Left - rulersSize,
                    clientRect.Top - rulersSize,
                    Settings.Environment.RulersSize,
                    Settings.Environment.RulersSize
                    );

                gfx.DrawString(
                    units.ShortName,
                    font,
                    Brushes.Black,
                    clientRect.Left - rulersSize + 1,
                    clientRect.Top - rulersSize + 1);
            }
        }
    }
}

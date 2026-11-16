using System;
using System.Drawing;
using Makarov.FlowchartBuilder.Extensions;

namespace Makarov.FlowchartBuilder
{

    public sealed partial class Core
    {

        private void DrawRulers(Graphics gfx, Rectangle clientRect, Point origin)
        {
            if (!Settings.Environment.Rulers) return;

            float scaleFactor = IsDocumentsExists ? CurrentDocument.DocumentSheet.ScaleFactor : 1f;
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

                float step = units.BigStep.CurrToPx() * scaleFactor * units.SmallStep / units.BigStep;
                DrawTicks(true, origin.X, clientRect.Width);
                DrawTicks(false, origin.Y, clientRect.Height);

                void DrawTicks(bool horizontal, int offset, int length)
                {
                    long first = (long)Math.Ceiling(offset / (double)step);
                    for (long tick = first; ; tick++)
                    {
                        float pixel = (float)(tick * (double)step - offset);
                        if (pixel >= length) break;

                        long value = tick * units.SmallStep;
                        bool major = value % units.BigStep == 0;
                        int tickLength = major ? rulersSize / 2 : rulersSize / 4;
                        float x = horizontal ? clientRect.Left + pixel : clientRect.Left - rulersSize;
                        float y = horizontal ? clientRect.Top - rulersSize : clientRect.Top + pixel;
                        gfx.DrawLine(Pens.Black, x, y, horizontal ? x : x + tickLength,
                                     horizontal ? y + tickLength : y);
                        if (major)
                            gfx.DrawString(value.ToString(), font, Brushes.Black,
                                x + (horizontal ? 1 : rulersSize / 4),
                                y + (horizontal ? rulersSize / 4 : 1));
                    }
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Sheets
{
    public sealed class InfiniteSheet : Sheet
    {
        public override Bitmap Draw(bool scaled)
        {
            Size size = Core.Instance.MainWindow.InnerRectangle.Size;
            if (scaled)
                return DrawRegion(size, ScaledViewOrigin);

            float previousScale = ScaleFactor;
            try
            {
                ScaleFactor = 1f;
                RectangleF bounds = GetContentBounds();
                if (bounds.IsEmpty)
                    return DrawRegion(size, ScaledViewOrigin);

                bounds.Inflate(16, 16);
                var origin = new Point((int)Math.Floor(bounds.Left), (int)Math.Floor(bounds.Top));
                size = new Size((int)Math.Ceiling(bounds.Right) - origin.X,
                                (int)Math.Ceiling(bounds.Bottom) - origin.Y);
                return DrawRegion(size, origin);
            }
            finally
            {
                ScaleFactor = previousScale;
            }
        }

        private Bitmap DrawRegion(Size size, Point origin)
        {
            var bmp = new Bitmap(size.Width, size.Height);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(Settings.Colors.Sheet);
                if (Settings.Environment.Grid)
                {
                    using var pen = (Pen)Settings.Pens.SheetGrid.Clone();
                    pen.DashPattern = new[] { 4f, 1f };
                    float step = Settings.Environment.CurrentUnits.BigStep.CurrToPx() * ScaleFactor;
                    for (float x = -origin.X % step; x < size.Width; x += step)
                        gfx.DrawLine(pen, x, 0, x, size.Height);
                    for (float y = -origin.Y % step; y < size.Height; y += step)
                        gfx.DrawLine(pen, 0, y, size.Width, y);
                }

                gfx.TranslateTransform(-origin.X, -origin.Y);
                var glyphs = new List<AbstractGlyph>(Glyphs);
                glyphs.Reverse();
                foreach (AbstractGlyph glyph in glyphs)
                    glyph.Draw(gfx);

                if (_selectionStart.HasValue)
                {
                    int x = _selectionStart.Value.X <= _selectionEnd.X
                                ? _selectionStart.Value.X
                                : _selectionEnd.X;

                    int y = _selectionStart.Value.Y <= _selectionEnd.Y
                                ? _selectionStart.Value.Y
                                : _selectionEnd.Y;

                    int width = Math.Abs(_selectionEnd.X - _selectionStart.Value.X);
                    int height = Math.Abs(_selectionEnd.Y - _selectionStart.Value.Y);

                    using (var brush = new SolidBrush(Settings.Colors.Selection))
                        gfx.FillRectangle(brush, x, y, width, height);

                    gfx.DrawRectangle(Settings.Pens.SelectionBorder, x, y, width, height);
                }

                if (_connectorSource != null && _connectorCurrent.HasValue)
                {
                    PointF start = _connectorSource.LinkPointsScaled[_connectorSourcePort];
                    Point end = _connectorCurrent.Value;

                    using (var pen = new Pen(Color.DodgerBlue, 1.5f))
                    {
                        pen.DashStyle = DashStyle.Dash;

                        using (var cap = new AdjustableArrowCap(4f, 5f, true))
                        {
                            pen.CustomEndCap = cap;
                            gfx.DrawLine(pen, start.X, start.Y, end.X, end.Y);
                        }
                    }
                }
            }

            return bmp;
        }

        public override object Clone()
        {
            return new InfiniteSheet();
        }
    }
}

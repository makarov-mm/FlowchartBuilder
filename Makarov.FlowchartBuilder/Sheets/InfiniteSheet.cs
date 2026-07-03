using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Sheets
{
    [SheetFamily("Custom")]
    [SheetName("Infinite")]
    public sealed class InfiniteSheet : Sheet
    {
        #region Properties

        public override Bitmap Thumbnail
        {
            get
            {
                var bmp = new Bitmap(ThumbnailWidth, ThumbnailHeight);

                using (Graphics gfx = Graphics.FromImage(bmp))
                {

                    int innerWidth = bmp.Width;
                    int innerHeight = bmp.Height;

                    using (var brush = new LinearGradientBrush(
                        new PointF(0, 0),
                        new PointF(innerWidth, innerHeight),
                        Settings.Colors.SheetGradient,
                        Color.White))
                    {
                        gfx.FillRectangle(
                            brush,
                            0, 0,
                            innerWidth, innerHeight);
                    }
                }

                bmp.DrawBorder(Settings.Colors.ThumbnailBorder);
                return bmp;
            }
        }
        #endregion

        #region Public methods

        public override Bitmap Draw(bool scaled)
        {
            float tmpScaleFactor = ScaleFactor;

            if (!scaled)
                ScaleFactor = 1f;

            int w = Core.Instance.MainWindow.InnerRectangle.Width;
            int h = Core.Instance.MainWindow.InnerRectangle.Height;

            var bmp = new Bitmap(w, h);

            using (Graphics gfx = Graphics.FromImage(bmp))
            {

                using (var brush = new LinearGradientBrush(
                    new Point(0, 0),
                    new Point(w, h),
                    Settings.Colors.Sheet,
                    Settings.Colors.SheetGradient))
                {
                    gfx.FillRectangle(brush, 0, 0, w, h);
                }

                if (Settings.Environment.Grid)
                    using (var pen = (Pen)Settings.Pens.SheetGrid.Clone())
                    {
                        pen.DashPattern = new[] { 4f, 1f };
                        float step = scaled
                                         ? Settings.Environment.CurrentUnits.BigStep.CurrToPx().Scale()
                                         : Settings.Environment.CurrentUnits.BigStep.CurrToPx();

                        for (float i = step; i < w; i += step)
                            gfx.DrawLine(pen, i, 0, i, h);

                        for (float i = step; i < h; i += step)
                            gfx.DrawLine(pen, 0, i, w, i);
                    }

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

            if (!scaled)
                ScaleFactor = tmpScaleFactor;

            return bmp;
        }

        public override object Clone()
        {
            return new InfiniteSheet();
        }
        #endregion
    }
}

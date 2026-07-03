using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Sheets
{
    [SheetWidth(210)]
    [SheetHeight(297)]
    public abstract class FixedSheet : Sheet, ISize, IScaledSize
    {
        #region Constructors

        protected FixedSheet()
        {
            if (DefaultWidthInMM <= 0 || DefaultHeightInMM <= 0)
                throw new InvalidSheetSizeException(DefaultWidthInMM, DefaultHeightInMM);

            Width = DefaultWidthInMM;
            Height = DefaultHeightInMM;
        }

        protected FixedSheet(float WidthInMM, float HeightInMM)
        {
            if (WidthInMM <= 0 || HeightInMM <= 0)
                throw new InvalidSheetSizeException(WidthInMM, HeightInMM);

            Width = WidthInMM;
            Height = HeightInMM;
        }
        #endregion

        #region Private members

        private float _width;

        private float _height;
        #endregion

        #region Properties

        public float DefaultWidthInMM
        {
            get
            {
                return ((SheetWidthAttribute)(GetType().GetCustomAttributes(typeof(SheetWidthAttribute), true)[0])).WidthInMM;
            }
        }

        public float DefaultHeightInMM
        {
            get
            {
                return ((SheetHeightAttribute)(GetType().GetCustomAttributes(typeof(SheetHeightAttribute), true)[0])).HeightInMM;
            }
        }

        public override Bitmap Thumbnail
        {
            get
            {
                var bmp = new Bitmap(ThumbnailWidth, ThumbnailHeight);

                using (Graphics gfx = Graphics.FromImage(bmp))
                {

                    const int innerOffsetX = 8;
                    const int innerOffsetY = 8;
                    int innerWidth = bmp.Width - innerOffsetX * 2;
                    int innerHeight = bmp.Height - innerOffsetY * 2;

                    float w = DefaultWidthInMM/500f*innerWidth;
                    float h = DefaultHeightInMM/500f*innerHeight;

                    const int shadowOffset = 2;

                    using (var brush = new SolidBrush(Settings.Colors.ThumbnailBorder))
                        gfx.FillRectangle(
                            brush,
                            shadowOffset + (innerWidth - w) / 2f + innerOffsetX,
                            shadowOffset + (innerHeight - h) / 2f + innerOffsetY,
                            w, h);

                    using (var brush = new LinearGradientBrush(
                        new PointF((innerWidth - w) / 2f + innerOffsetX, (innerHeight - h) / 2f + innerOffsetY),
                        new PointF((innerWidth - w) / 2f + innerOffsetX + w, (innerHeight - h) / 2f + innerOffsetY + h),
                        Settings.Colors.SheetGradient,
                        Color.White))
                    {
                        gfx.FillRectangle(
                            brush,
                            (innerWidth - w)/2f + innerOffsetX,
                            (innerHeight - h)/2f + innerOffsetY,
                            w, h);
                    }

                    gfx.DrawRectangle(
                        Settings.Pens.ThumbnailBorder,
                        (innerWidth - w) / 2f + innerOffsetX,
                        (innerHeight - h) / 2f + innerOffsetY,
                        w, h);

                }

                bmp.DrawBorder(Settings.Colors.ThumbnailBorder);
                return bmp;
            }
        }
        #endregion

        #region ISize

        public float Width
        {
            get { return _width; }

            set
            {
                if (value <= 0)
                    throw new InvalidValueException("Sheet.Width", value.ToString());

                _width = value;
            }
        }

        public float Height
        {
            get { return _height; }

            set
            {
                if (value <= 0)
                    throw new InvalidValueException("Sheet.Height", value.ToString());

                _height = value;
            }
        }
        #endregion

        #region IScaledSize

        public float ScaledWidth
        {
            get { return Width.Scale(); }
            set { Width = value.Unscale(); }
        }

        public float ScaledHeight
        {
            get { return Height.Scale(); }
            set { Height = value.Unscale(); }
        }
        #endregion

        #region Public methods

        public override Bitmap Draw(bool scaled)
        {
            float tmpScaleFactor = ScaleFactor;

            if (!scaled)
                ScaleFactor = 1f;

            var bmp = scaled
                          ? new Bitmap(ScaledWidth.MMToPx(), ScaledHeight.MMToPx())
                          : new Bitmap(Width.MMToPx(), Height.MMToPx());

            using (Graphics gfx = Graphics.FromImage(bmp))
            {

                using (var brush = new LinearGradientBrush(
                    new Point(0, 0),
                    scaled
                        ? new Point(ScaledWidth.MMToPx(), ScaledHeight.MMToPx())
                        : new Point(Width.MMToPx(), Height.MMToPx()),
                    Settings.Colors.Sheet,
                    Settings.Colors.SheetGradient))
                {
                    gfx.FillRectangle(
                        brush,
                        0, 0,
                        scaled ? ScaledWidth.MMToPx() : Width.MMToPx(),
                        scaled ? ScaledHeight.MMToPx() : Height.MMToPx());
                }

                if (Settings.Environment.Grid)
                    using (var pen = (Pen) Settings.Pens.SheetGrid.Clone())
                    {
                        pen.DashPattern = new[] {4f, 1f};
                        float step = scaled
                                         ? Settings.Environment.CurrentUnits.BigStep.CurrToPx().Scale()
                                         : Settings.Environment.CurrentUnits.BigStep.CurrToPx();

                        for (float i = step; i < ScaledWidth.MMToPx(); i += step)
                            gfx.DrawLine(pen, i, 0, i, scaled ? ScaledHeight.MMToPx() : Height.MMToPx());

                        for (float i = step; i < ScaledHeight.MMToPx(); i += step)
                            gfx.DrawLine(pen, 0, i, scaled ? ScaledWidth.MMToPx() : Width.MMToPx(), i);
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
            var tmp = (FixedSheet) Activator.CreateInstance(GetType());

            foreach (var glyph in Glyphs)
            {
                var clone = (AbstractGlyph) glyph.Clone();
                clone.Selected = false;
                tmp.Add(clone);
            }

            return tmp;
        }
        #endregion
    }
}

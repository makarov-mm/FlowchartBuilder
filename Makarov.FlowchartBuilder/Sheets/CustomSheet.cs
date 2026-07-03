using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.FlowchartBuilder.Extensions;

namespace Makarov.FlowchartBuilder.Sheets
{
    [SheetFamily("Custom")]
    [SheetName("Custom")]
    public sealed class CustomSheet : Sheet
    {
        #region Properties

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

                    float w = 210f / 500f * innerWidth;
                    float h = 297f / 500f * innerHeight;

                    using (var brush = new LinearGradientBrush(
                        new PointF((innerWidth - w) / 2f + innerOffsetX, (innerHeight - h) / 2f + innerOffsetY),
                        new PointF((innerWidth - w) / 2f + innerOffsetX + w, (innerHeight - h) / 2f + innerOffsetY + h),
                        Settings.Colors.SheetGradient,
                        Color.White))
                    {
                        gfx.FillRectangle(
                            brush,
                            (innerWidth - w) / 2f + innerOffsetX,
                            (innerHeight - h) / 2f + innerOffsetY,
                            w, h);
                    }

                    using (var pen = new Pen(Settings.Colors.ThumbnailBorder))
                    {
                        pen.DashPattern = new float[] {4, 2};
                        gfx.DrawRectangle(
                            pen,
                            (innerWidth - w)/2f + innerOffsetX,
                            (innerHeight - h)/2f + innerOffsetY,
                            w, h);
                    }
                }

                bmp.DrawBorder(Settings.Colors.ThumbnailBorder);
                return bmp;
            }
        }
        #endregion

        #region Public methods

        public override Bitmap Draw(bool Scaled)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            return new CustomSheet();
        }
        #endregion
    }
}

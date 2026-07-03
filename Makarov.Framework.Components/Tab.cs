using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Makarov.Framework.Components
{

    public sealed class Tab
    {

        public Tab(int id, string caption)
        {
            Id = id;
            Caption = caption ?? string.Empty;
        }

        public int Id { get; private set; }

        public string Caption { get; private set; }

        public int RequiredWidth(Graphics gfx, Font font)
        {
            return (int)Math.Ceiling(gfx.MeasureString(Caption, font).Width);
        }

        public int RequiredHeight(Graphics gfx, Font font)
        {
            return (int) Math.Ceiling(gfx.MeasureString(Caption, font).Height);
        }

        public Size RequiredSize(Graphics gfx, Font font)
        {
            return new Size(
                RequiredWidth(gfx, font),
                RequiredHeight(gfx, font));
        }

        public GraphicsPath CreatePath(Graphics gfx, Font font,
            int? minWidth, int? maxWidth, int? minHeight, int? maxHeight)
        {
            int width = RequiredWidth(gfx, font);

            if (minWidth.HasValue && minWidth.Value > width)
                width = minWidth.Value;

            if (maxWidth.HasValue && maxWidth.Value < width)
                width = maxWidth.Value;

            int height = RequiredWidth(gfx, font);

            if (minHeight.HasValue && minHeight.Value > height)
                height = minHeight.Value;

            if (maxHeight.HasValue && maxHeight.Value < height)
                height = maxHeight.Value;

            const int offset = 2;
            const int cornerSize = 5;

            var pts = new[]
            {
                new Point(0, height + offset),
                new Point(width, height + offset),
                new Point(width, 0),
                new Point(cornerSize, 0),
                new Point(0, cornerSize),
                new Point(0, height + offset)
            };

            var path = new GraphicsPath();
            path.StartFigure();
            path.AddLines(pts);
            path.CloseFigure();

            return path;
        }

        public void Draw(Graphics gfx, int offsetX, int offsetY, Font font,
            int? minWidth, int? maxWidth, int? minHeight, int? maxHeight, bool isActive)
        {
            gfx.TranslateTransform(offsetX, offsetY);

            using (GraphicsPath path = CreatePath(gfx, font, minWidth, maxWidth, minHeight, maxHeight))
            using (var gradientBrush = new LinearGradientBrush(
                    new Point(0, 2),
                    new Point(0, (int)path.GetBounds().Height),
                    SystemColors.Control,
                    isActive ? SystemColors.ControlDark : SystemColors.ControlLight))
            using (var borderPen = new Pen(SystemColors.ActiveBorder, 1f))
            {
                gfx.FillPath(gradientBrush, path);
                gfx.DrawPath(borderPen, path);

                SizeF textSize = gfx.MeasureString(Caption, font);
                int textOffsetX = (int)(path.GetBounds().Width - textSize.Width) >> 1;
                int textOffsetY = (int)(path.GetBounds().Height - textSize.Height) >> 1;

                gfx.DrawString(Caption, font, Brushes.Black, textOffsetX, textOffsetY);
            }

            gfx.TranslateTransform(-offsetX, -offsetY);
        }
    }
}

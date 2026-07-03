using System.Drawing;
using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives
{

    public class Arrow : Primitive
    {
        #region Constructors

        public Arrow(int x, int y, int width, int height)
            : base(x, y, width, height)
        { }

        public Arrow(Point coords, Size size)
            : base(coords.X, coords.Y, size.Width, size.Height)
        { }

        public Arrow(System.Drawing.Rectangle rect)
            : base(rect.X, rect.Y, rect.Width, rect.Height)
        { }
        #endregion

        #region Protected methods

        protected override GraphicsPath CreatePath(int x, int y, int width, int height)
        {
            var path = new GraphicsPath();

            path.StartFigure();
            path.AddLines(
                new[]
                {
                    new PointF(x + (width >> 1), y),
                    new PointF(x + width, y + (height >> 1)),
                    new PointF(x + width  / 3 * 2, y + (height >> 1)),
                    new PointF(x + width  / 3 * 2, y + height),
                    new PointF(x + width  / 3, y + height),
                    new PointF(x + width  / 3, y + (height >> 1)),
                    new PointF(x, y + (height >> 1)),
                    new PointF(x + (width >> 1), y)
                }
            );
            path.CloseFigure();

            return path;
        }
        #endregion
    }
}

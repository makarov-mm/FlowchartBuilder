using System.Drawing;
using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives
{

    public class Circle : Primitive
    {
        #region Constructors

        public Circle(int x, int y, int width, int height)
            : base(x, y, width, height)
        { }

        public Circle(Point coords, Size size)
            : base(coords.X, coords.Y, size.Width, size.Height)
        { }

        public Circle(System.Drawing.Rectangle rect)
            : base(rect.X, rect.Y, rect.Width, rect.Height)
        { }
        #endregion

        #region Protected methods

        protected override GraphicsPath CreatePath(int x, int y, int width, int height)
        {
            var path = new GraphicsPath();

            path.StartFigure();
            path.AddArc(x, y, width, height, 0, 360);
            path.CloseFigure();

            return path;
        }
        #endregion
    }
}

using System.Drawing;
using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives
{

    public class DirectTriangle : Primitive
    {
        #region Constructors

        public DirectTriangle(int x, int y, int width, int height)
            : base(x, y, width, height)
        { }

        public DirectTriangle(Point coords, Size size)
            : base(coords.X, coords.Y, size.Width, size.Height)
        { }

        public DirectTriangle(System.Drawing.Rectangle rect)
            : base(rect.X, rect.Y, rect.Width, rect.Height)
        { }
        #endregion

        #region Protected methods

        protected override GraphicsPath CreatePath(int x, int y, int width, int height)
        {
            var path = new GraphicsPath();

            path.StartFigure();
            path.AddLine(x, y, x + width, y + height);
            path.AddLine(x + width, y + height, x, y + height);
            path.AddLine(x, y + height, x, y);
            path.CloseFigure();

            return path;
        }
        #endregion
    }
}

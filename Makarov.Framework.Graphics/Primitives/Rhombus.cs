using System.Drawing;
using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives
{

    public class Rhombus : Primitive
    {
        #region Constructors

        public Rhombus(int x, int y, int width, int height)
            : base(x, y, width, height)
        { }

        public Rhombus(Point coords, Size size)
            : base(coords.X, coords.Y, size.Width, size.Height)
        { }

        public Rhombus(System.Drawing.Rectangle rect)
            : base(rect.X, rect.Y, rect.Width, rect.Height)
        { }
        #endregion

        #region Protected methods

        protected override GraphicsPath CreatePath(int x, int y, int width, int height)
        {
            var path = new GraphicsPath();

            path.StartFigure();
            path.AddLine(x + width / 2, y, x + width, y + height / 2);
            path.AddLine(x + width, y + height / 2, x + width / 2, y + height);
            path.AddLine(x + width / 2, y + height, x, y + height / 2);
            path.AddLine(x, y + height / 2, x + width / 2, y);
            path.CloseFigure();

            return path;
        }
        #endregion
    }
}

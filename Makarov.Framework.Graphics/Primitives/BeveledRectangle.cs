using System.Drawing;
using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives
{

    public class BeveledRectangle : Primitive
    {
        #region Constructors

        public BeveledRectangle(int x, int y, int width, int height, int offsetX)
            : base(x, y, width, height)
        {
            OffsetX = offsetX;
        }

        public BeveledRectangle(Point coords, Size size, int offsetX)
            : base(coords.X, coords.Y, size.Width, size.Height)
        {
            OffsetX = offsetX;
        }

        public BeveledRectangle(System.Drawing.Rectangle rect, int offsetX)
            : base(rect.X, rect.Y, rect.Width, rect.Height)
        {
            OffsetX = offsetX;
        }
        #endregion

        #region Protected methods

        protected override GraphicsPath CreatePath(int x, int y, int width, int height)
        {
            var path = new GraphicsPath();

            path.StartFigure();
            path.AddLine(x + OffsetX, y, x + width, y);
            path.AddLine(x + width, y, x + width - OffsetX, y + height);
            path.AddLine(x + width - OffsetX, y + height, x, y + height);
            path.AddLine(x, y + height, x + OffsetX, y);
            path.CloseFigure();

            return path;
        }
        #endregion

        #region Properties

        public int OffsetX
        {
            get; set;
        }
        #endregion
    }
}

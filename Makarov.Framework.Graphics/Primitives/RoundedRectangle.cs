using System.Drawing;
using System.Drawing.Drawing2D;
using Makarov.Framework.Core;

namespace Makarov.Framework.Graphics.Primitives
{

    public class RoundedRectangle : Primitive
    {
        #region Exceptions

        public class InvalidDiameterException : MakarovFrameworkException
        {

            public InvalidDiameterException(float d)
                : base(string.Format("Invalid diameter: {0}.", d))
            { }
        }
        #endregion

        #region Constructors

        public RoundedRectangle(int x, int y, int width, int height, float d)
            : base(x, y, width, height)
        {
            Diametr = d;
        }

        public RoundedRectangle(Point coords, Size size, float d)
            : base(coords.X, coords.Y, size.Width, size.Height)
        {
            Diametr = d;
        }

        public RoundedRectangle(System.Drawing.Rectangle rect, float d)
            : base(rect.X, rect.Y, rect.Width, rect.Height)
        {
            Diametr = d;
        }
        #endregion

        #region Protected methods

        protected override GraphicsPath CreatePath(int x, int y, int width, int height)
        {
            var path = new GraphicsPath();
            float r = Diametr / 2f;

            path.StartFigure();
            path.AddLine(x + r, y, x + width - r, y);
            path.AddArc(x + width - Diametr, y, Diametr, Diametr, 270, 90);
            path.AddLine(x + width, y + r, x + width, y + height - r);
            path.AddArc(x + width - Diametr, y + height - Diametr, Diametr, Diametr, 0, 90);
            path.AddLine(x + width - r, y + height, x + r, y + height);
            path.AddArc(x, y + height - Diametr, Diametr, Diametr, 90, 90);
            path.AddLine(x, y + height - r, x, y + r);
            path.AddArc(x, y, Diametr, Diametr, 180, 90);
            path.CloseFigure();

            return path;
        }
        #endregion

        #region Private members

        private float _d;
        #endregion

        #region Properties
        public float Diametr
        {
            get { return _d; }
            set
            {
                if (value <= 0)
                    throw new InvalidDiameterException(value);

                _d = value;
            }
        }
        #endregion
    }
}

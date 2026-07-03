using System.Drawing;
using System.Drawing.Drawing2D;
using Makarov.Framework.Core;

namespace Makarov.Framework.Graphics.Primitives
{

    public abstract class Primitive
    {
        #region Constructors

        protected Primitive(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        protected Primitive(Point coords, Size size)
            : this(coords.X, coords.Y, size.Width, size.Height)
        { }

        protected Primitive(System.Drawing.Rectangle rect)
            : this(rect.X, rect.Y, rect.Width, rect.Height)
        { }
        #endregion

        #region Exceptions

        public class InvalidPrimitiveWidthException : MakarovFrameworkException
        {

            public InvalidPrimitiveWidthException(int width)
                : base(string.Format("Invalid width = '{0}'", width))
            { }
        }

        public class InvalidPrimitiveHeightException : MakarovFrameworkException
        {

            public InvalidPrimitiveHeightException(int height)
                : base(string.Format("Invalid height = '{0}'", height))
            { }
        }
        #endregion

        #region Private members

        private int _width;

        private int _height;
        #endregion

        #region Properties

        public int X { get; set; }

        public int Y { get; set; }

        public int Width
        {
            get { return _width; }
            set
            {
                if (value <= 0)
                    throw new InvalidPrimitiveWidthException(value);

                _width = value;
            }
        }

        public int Height
        {
            get { return _height; }
            set
            {
                if (value <= 0)
                    throw new InvalidPrimitiveHeightException(value);

                _height = value;
            }
        }
        #endregion

        #region Public methods

        public GraphicsPath Create()
        {
            return CreatePath(X, Y, Width, Height);
        }

        public GraphicsPath CreateCentered()
        {
            return CreatePath(X - (Width >> 1), Y - (Height >> 1), Width, Height);
        }
        #endregion

        #region Protected methods

        protected abstract GraphicsPath CreatePath(int x, int y, int width, int height);
        #endregion
    }
}

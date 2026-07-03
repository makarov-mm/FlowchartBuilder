using System.Drawing;
using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives
{

    public class Pie : Primitive
    {
        #region Constructors

        public Pie(int x, int y, int width, int height, int startAngle, int angle)
            : base(x, y, width, height)
        {
            StartAngle = startAngle;
            Angle = angle;
        }

        public Pie(Point coords, Size size, int startAngle, int angle)
            : base(coords.X, coords.Y, size.Width, size.Height)
        {
            StartAngle = startAngle;
            Angle = angle;
        }

        public Pie(System.Drawing.Rectangle rect, int startAngle, int angle)
            : base(rect.X, rect.Y, rect.Width, rect.Height)
        {
            StartAngle = startAngle;
            Angle = angle;
        }
        #endregion

        #region Protected methods

        protected override GraphicsPath CreatePath(int x, int y, int width, int height)
        {
            var path = new GraphicsPath();

            path.StartFigure();
            path.AddPie(x, y, width, height, StartAngle, Angle);
            path.CloseFigure();

            return path;
        }
        #endregion

        #region Properties

        public int StartAngle
        {
            get;
            set;
        }

        public int Angle
        {
            get;
            set;
        }
        #endregion
    }
}

using System.Drawing;

namespace Makarov.Framework.Graphics
{

    public struct Point2D
    {

        public Point2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X;

        public float Y;

        public static implicit operator Point2D (Point pt)
        {
            return new Point2D(pt.X, pt.Y);
        }

        public static implicit operator PointF(Point2D pt)
        {
            return new PointF(pt.X, pt.Y);
        }

        public static explicit operator Point (Point2D pt)
        {
            return new Point((int)pt.X, (int)pt.Y);
        }
    }
}

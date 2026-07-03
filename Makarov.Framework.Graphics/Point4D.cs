namespace Makarov.Framework.Graphics
{

    public struct Point4D
    {

        public Point4D(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public float X;

        public float Y;

        public float Z;

        public float W;

        public Point4D Normalize()
        {
            return new Point4D(X/W, Y/W, Z/W, 1f);
        }
    }
}

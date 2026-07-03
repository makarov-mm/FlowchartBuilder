using System.Drawing;

namespace Makarov.Framework.Graphics
{

    public struct ColorF
    {
        public ColorF(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public ColorF(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
            A = 1f;
        }

        public ColorF(Color color)
        {
            R = color.R/255f;
            G = color.G/255f;
            B = color.B/255f;
            A = color.A/255f;
        }

        public float R;

        public float G;

        public float B;

        public float A;

        public float[] ToArray()
        {
            return new[] {R, G, B, A};
        }

        public static implicit operator ColorF(Color color)
        {
            return new ColorF(color);
        }

        public static explicit operator Color (ColorF color)
        {
            return Color.FromArgb(
                (int) (color.A*255f),
                (int) (color.R*255f),
                (int) (color.G*255f),
                (int) (color.B*255f));
        }
    }
}

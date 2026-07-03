namespace Makarov.Framework.Core
{

    public static class Math
    {

        public static float DegToRad(this float deg)
        {
            return (float)System.Math.PI / 180f * deg;
        }

        public static double DegToRad(this double deg)
        {
            return System.Math.PI / 180.0 * deg;
        }

        public static float RadToDeg(this float rad)
        {
            return 180f / (float)System.Math.PI * rad;
        }

        public static double RadToDeg(this double rad)
        {
            return 180.0 / System.Math.PI * rad;
        }
    }
}

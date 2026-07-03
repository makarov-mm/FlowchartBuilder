using System;

namespace Makarov.Framework.Core.Extensions
{

    public static class Int32Extensions
    {
        #region TimeSpan

        public static TimeSpan AsDays(this int d)
        {
            return new TimeSpan(d, 0, 0, 0);
        }

        public static TimeSpan AsHours(this int h)
        {
            return new TimeSpan(0, h, 0, 0);
        }

        public static TimeSpan AsMinutes(this int m)
        {
            return new TimeSpan(0, 0, m, 0);
        }

        public static TimeSpan AsSeconds(this int s)
        {
            return new TimeSpan(0, 0, 0, s);
        }
        #endregion

        #region Predicates

        public static bool IsEven(this int x)
        {
            return x % 2 == 0;
        }

        public static bool IsOdd(this int x)
        {
            return x % 2 != 0;
        }
        #endregion
    }
}

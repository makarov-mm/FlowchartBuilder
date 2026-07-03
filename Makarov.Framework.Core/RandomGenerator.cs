using System;

namespace Makarov.Framework.Core
{

    public sealed class RandomGenerator : Singleton<RandomGenerator>
    {

        public sealed class MinGreaterThanMaxException : MakarovFrameworkException
        {

            public MinGreaterThanMaxException()
                : base("Min value greater than max value.")
            { }

            public MinGreaterThanMaxException(string message, Exception innerException)
                : base(message, innerException)
            { }
        }

        private readonly Random _rnd;

        private RandomGenerator()
        {

            unchecked
            {
                var now = DateTime.Now;

                _rnd = new Random(
                    (int)now.Ticks +
                    now.Millisecond * 60 * 60 * 100 +
                    now.Second * 60 * 60 +
                    now.Minute * 60 +
                    now.Hour);
            }
        }

        public int Next()
        {
            return _rnd.Next();
        }

        public int Next(int maxValue)
        {
            return _rnd.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {

            if (minValue > maxValue)
                throw new MinGreaterThanMaxException();

            return _rnd.Next(minValue, maxValue);
        }

        public int Next(Range range)
        {
            return _rnd.Next(range.MinValue, range.MaxValue + 1);
        }

        public byte NextByte()
        {
            return (byte)_rnd.Next(256);
        }

        public double NextDouble()
        {
            return _rnd.NextDouble();
        }

        public float NextFloat()
        {
            return (float)_rnd.NextDouble();
        }

        public double NextDouble(double maxValue)
        {
            return _rnd.NextDouble() * maxValue;
        }

        public float NextFloat(float maxValue)
        {
            return (float)(_rnd.NextDouble() * maxValue);
        }

        public double NextDouble(double minValue, double maxValue)
        {

            if (minValue > maxValue)
                throw new MinGreaterThanMaxException();

            return _rnd.NextDouble() * (maxValue - minValue) + minValue;
        }

        public float NextFloat(float minValue, float maxValue)
        {

            if (minValue > maxValue)
                throw new MinGreaterThanMaxException();

            return (float)(_rnd.NextDouble() * (maxValue - minValue) + minValue);
        }
    }
}

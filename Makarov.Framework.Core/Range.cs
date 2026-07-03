using System;

namespace Makarov.Framework.Core
{

    public sealed class Range
    {
        public sealed class RangeException : MakarovFrameworkException
        {
            public RangeException(string message)
                : base(message ?? string.Empty)
            { }
        }

        public Range(int value)
            : this(value, value)
        { }

        public Range(int minValue, int maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;

            if (maxValue < minValue)
                throw new RangeException($"{nameof(maxValue)} must be greater or equal than {nameof(minValue)}.");
        }

        public int MinValue
        {
            get;
        }

        public int MaxValue
        {
            get;
        }

        public int Count => MaxValue - MinValue + 1;

        public bool CanBeCombinedWith(Range range)
        {
            return !(MaxValue < range.MinValue - 1 ||
                     MinValue > range.MaxValue + 1);
        }

        public Range Combine(Range range)
        {
            if (!CanBeCombinedWith(range))
                throw new RangeException("Ranges can't be combined.");

            return new Range(
                System.Math.Min(MinValue, range.MinValue),
                System.Math.Max(MaxValue, range.MaxValue));
        }

        public void ForEach(Action<int> action)
        {
            for (int i = MinValue; i <= MaxValue; i++)
                action(i);
        }

        public static Range operator +(Range range1, Range range2)
        {
            return range1.Combine(range2);
        }
    }
}

using System;
using System.Globalization;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class DoubleSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(double);

        public override string Serialize(object obj)
        {
            return ((double)obj).ToString("r", CultureInfo.InvariantCulture);
        }

        public override object Deserialize(string str)
        {
            return double.Parse(str, CultureInfo.InvariantCulture);
        }
    }
}

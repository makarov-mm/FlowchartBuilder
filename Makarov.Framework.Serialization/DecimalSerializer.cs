using System;
using System.Globalization;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class DecimalSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(decimal);

        public override string Serialize(object obj)
        {
            return ((decimal)obj).ToString("F", CultureInfo.InvariantCulture);
        }

        public override object Deserialize(string str)
        {
            return decimal.Parse(str, CultureInfo.InvariantCulture);
        }
    }
}

using System;
using System.Globalization;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class FloatSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(float);

        public override string Serialize(object obj)
        {
            return ((float)obj).ToString("r", CultureInfo.InvariantCulture);
        }

        public override object Deserialize(string str)
        {
            return float.Parse(str, CultureInfo.InvariantCulture);
        }
    }
}

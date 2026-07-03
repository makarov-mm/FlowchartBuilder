using System;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class ShortSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(short);

        public override string Serialize(object obj)
        {
            return ((short)obj).ToString();
        }

        public override object Deserialize(string str)
        {
            return short.Parse(str);
        }
    }
}

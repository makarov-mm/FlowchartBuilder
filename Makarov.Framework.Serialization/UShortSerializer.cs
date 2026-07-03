using System;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class UShortSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(ushort);

        public override string Serialize(object obj)
        {
            return ((ushort)obj).ToString();
        }

        public override object Deserialize(string str)
        {
            return ushort.Parse(str);
        }
    }
}

using System;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class ByteSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(byte);

        public override string Serialize(object obj)
        {
            return ((byte)obj).ToString();
        }

        public override object Deserialize(string str)
        {
            return byte.Parse(str);
        }
    }
}

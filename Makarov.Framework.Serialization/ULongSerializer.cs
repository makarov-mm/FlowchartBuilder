using System;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class ULongSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(ulong);

        public override string Serialize(object obj)
        {
            return ((ulong)obj).ToString();
        }

        public override object Deserialize(string str)
        {
            return ulong.Parse(str);
        }
    }
}

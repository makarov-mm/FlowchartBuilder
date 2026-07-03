using System;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class UIntSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(uint);

        public override string Serialize(object obj)
        {
            return ((uint)obj).ToString();
        }

        public override object Deserialize(string str)
        {
            return uint.Parse(str);
        }
    }
}

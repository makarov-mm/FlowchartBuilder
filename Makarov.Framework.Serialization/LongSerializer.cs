using System;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class LongSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(long);

        public override string Serialize(object obj)
        {
            return ((long)obj).ToString();
        }

        public override object Deserialize(string str)
        {
            return long.Parse(str);
        }
    }
}

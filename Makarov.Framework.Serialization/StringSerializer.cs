using System;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class StringSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(string);

        public override string Serialize(object obj)
        {
            return (string) obj;
        }

        public override object Deserialize(string str)
        {
            return str;
        }
    }
}

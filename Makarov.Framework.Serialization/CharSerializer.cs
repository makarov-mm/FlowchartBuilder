using System;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class CharSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(char);

        public override string Serialize(object obj)
        {
            return ((char)obj).ToString();
        }

        public override object Deserialize(string str)
        {
            return str[0];
        }
    }
}

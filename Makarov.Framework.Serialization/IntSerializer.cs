using System;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class IntSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof (int);

        public override string Serialize(object obj)
        {
            return ((int) obj).ToString();
        }

        public override object Deserialize(string str)
        {
            return int.Parse(str);
        }
    }
}

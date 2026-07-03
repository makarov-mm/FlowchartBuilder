using System;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class VersionSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(Version);

        public override string Serialize(object obj)
        {
            return obj.ToString();
        }

        public override object Deserialize(string str)
        {
            return new Version(str);
        }
    }
}

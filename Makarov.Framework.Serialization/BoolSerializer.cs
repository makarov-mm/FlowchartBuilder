using System;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class BoolSerializer : AbstractSerializer
    {

        public override Type SerializerType
        {
            get { return typeof(bool); }
        }

        public override string Serialize(object obj)
        {
            var b = (bool) obj;
            if (b) return @"True";
            return @"False";
        }

        public override object Deserialize(string str)
        {
            if (str == @"True" ||
                str == @"TRUE" ||
                str == @"true" ||
                str.ToLowerInvariant() == @"true" ||
                str.ToUpperInvariant() == @"TRUE")
                return true;

            if (str == @"False" ||
                str == @"FALSE" ||
                str == @"false" ||
                str.ToLowerInvariant() == @"false" ||
                str.ToUpperInvariant() == @"FALSE")
                return false;

            if (str == bool.TrueString)
                return true;

            if (str == bool.FalseString)
                return false;

            return bool.Parse(str);
        }
    }
}

using System;
using System.Drawing;
using System.Globalization;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class FontSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(Font);

        public override string Serialize(object obj)
        {
            var font = (Font)obj;
            return font.Name + ";" + font.Size.ToString("r", CultureInfo.InvariantCulture);
        }

        public override object Deserialize(string str)
        {
            string[] pairs = str.Split(';');
            return Fonts.SafeCreateFont(pairs[0], float.Parse(pairs[1], CultureInfo.InvariantCulture));
        }
    }
}

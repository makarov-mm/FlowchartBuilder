using System;
using System.Drawing;
using System.Globalization;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization
{
    public sealed class ColorSerializer : AbstractSerializer
    {

        public override Type SerializerType => typeof(Color);

        public override string Serialize(object obj)
        {
            var color = (Color)obj;
            return $"{color.R};{color.G};{color.B};{color.A}";
        }

        public override object Deserialize(string str)
        {
            var arr = str.Split(';');
            return Color.FromArgb(
                byte.Parse(arr[3]),
                byte.Parse(arr[0]),
                byte.Parse(arr[1]),
                byte.Parse(arr[2]));
        }
    }
}

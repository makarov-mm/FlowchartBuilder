using System.Globalization;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization;

/// <summary>Format: "R;G;B;A".</summary>
public sealed class ColorSerializer : AbstractSerializer<Color>
{
    protected override string Format(Color value) =>
        string.Create(CultureInfo.InvariantCulture, $"{value.R};{value.G};{value.B};{value.A}");

    protected override Color Parse(string text)
    {
        string[] parts = text.Split(';');
        return Color.FromArgb(
            ParseByte(parts[3]),
            ParseByte(parts[0]),
            ParseByte(parts[1]),
            ParseByte(parts[2]));
    }

    private static byte ParseByte(string text) => byte.Parse(text, CultureInfo.InvariantCulture);
}

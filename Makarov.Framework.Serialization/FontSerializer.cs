using System.Globalization;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization;

/// <summary>
/// Format: "Family;Size;Style". The style part is optional, so values written by older versions still load,
/// and older versions simply ignore it.
/// </summary>
public sealed class FontSerializer : AbstractSerializer<Font>
{
    protected override string Format(Font value) =>
        string.Create(CultureInfo.InvariantCulture, $"{value.Name};{value.Size};{value.Style}");

    protected override Font Parse(string text)
    {
        string[] parts = text.Split(';');
        float size = float.Parse(parts[1], CultureInfo.InvariantCulture);
        FontStyle style = parts.Length > 2 && Enum.TryParse(parts[2], out FontStyle parsed) ? parsed : FontStyle.Regular;

        return Fonts.Create(parts[0], size, style);
    }
}

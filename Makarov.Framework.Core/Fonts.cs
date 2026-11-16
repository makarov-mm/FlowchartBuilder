namespace Makarov.Framework.Core;

public static class Fonts
{
    private static readonly FontStyle[] FallbackStyles =
        [FontStyle.Regular, FontStyle.Italic, FontStyle.Bold, FontStyle.Underline, FontStyle.Strikeout];

    /// <summary>
    /// Creates a font in the requested style, or in the first style the family supports
    /// (some families have no regular face and throw on <see cref="FontStyle.Regular"/>).
    /// </summary>
    public static Font Create(string familyName, float emSize, FontStyle style = FontStyle.Regular)
    {
        if (TryCreate(familyName, emSize, style) is { } font)
            return font;

        foreach (FontStyle fallback in FallbackStyles)
        {
            if (TryCreate(familyName, emSize, fallback) is { } fallbackFont)
                return fallbackFont;
        }

        return new Font(FontFamily.GenericSansSerif, emSize, style);
    }

    private static Font? TryCreate(string familyName, float emSize, FontStyle style)
    {
        try
        {
            return new Font(familyName, emSize, style);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}

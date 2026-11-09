namespace Makarov.Framework.Graphics;

/// <summary>Font for <see cref="Canvas"/> text: family, size in points and style.</summary>
public sealed record TextStyle(string Family, float SizeInPoints, bool Bold = false, bool Italic = false)
{
    public bool Underline { get; init; }

    public bool Strikeout { get; init; }

    /// <summary>Font size in device-independent pixels (1/96 inch).</summary>
    internal float SizeInDips => SizeInPoints * 96f / 72f;

    public static TextStyle FromFont(Font font) =>
        new(font.FontFamily.Name, font.SizeInPoints, font.Bold, font.Italic)
        {
            Underline = font.Underline,
            Strikeout = font.Strikeout,
        };

    /// <summary>The same style with the size multiplied by <paramref name="factor"/>, for zoomed drawings.</summary>
    public TextStyle Scale(float factor) => this with { SizeInPoints = SizeInPoints * factor };
}

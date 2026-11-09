using Makarov.Framework.Graphics.Interop;

namespace Makarov.Framework.Graphics;

/// <summary>DirectWrite text formats (cached for the process lifetime) and per-call text layouts.</summary>
internal static class TextLayouts
{
    /// <summary>Layout limit used when the text is not bounded.</summary>
    public const float Unbounded = 100_000f;

    private static readonly Dictionary<(string, bool, bool, float), nint> Formats = [];

    /// <summary>Creates a text layout; the caller releases it.</summary>
    public static nint Create(
        string text, TextStyle style, float maxWidth, float maxHeight, TextAlignment alignment, ParagraphAlignment paragraph, bool wrap)
    {
        nint layout = DirectWrite.CreateTextLayout(text, GetFormat(style), maxWidth, maxHeight);

        try
        {
            DirectWrite.SetTextAlignment(layout, (int)alignment);
            DirectWrite.SetParagraphAlignment(layout, (int)paragraph);
            DirectWrite.SetWordWrapping(layout, wrap ? Native.DWRITE_WORD_WRAPPING_WRAP : Native.DWRITE_WORD_WRAPPING_NO_WRAP);

            var all = new Native.TextRange(0, (uint)text.Length);
            if (style.Underline)
                DirectWrite.SetUnderline(layout, true, all);

            if (style.Strikeout)
                DirectWrite.SetStrikethrough(layout, true, all);

            return layout;
        }
        catch
        {
            Com.Release(ref layout);
            throw;
        }
    }

    public static SizeF Measure(string text, TextStyle style, float maxWidth)
    {
        if (string.IsNullOrEmpty(text))
            return SizeF.Empty;

        nint layout = Create(text, style, maxWidth, Unbounded, TextAlignment.Leading, ParagraphAlignment.Near, wrap: maxWidth < Unbounded);

        try
        {
            Native.TextMetrics metrics = DirectWrite.GetMetrics(layout);
            return new SizeF(metrics.WidthIncludingTrailingWhitespace, metrics.Height);
        }
        finally
        {
            Com.Release(ref layout);
        }
    }

    private static nint GetFormat(TextStyle style)
    {
        var key = (style.Family, style.Bold, style.Italic, style.SizeInDips);

        lock (Formats)
        {
            if (!Formats.TryGetValue(key, out nint format))
            {
                format = DirectWrite.CreateTextFormat(style.Family, style.Bold, style.Italic, Math.Max(style.SizeInDips, 0.5f));
                Formats.Add(key, format);
            }

            return format;
        }
    }
}

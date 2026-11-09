using System.Globalization;

namespace Makarov.Framework.Graphics.Interop;

/// <summary>The shared IDWriteFactory and the text format and text layout methods the renderer uses.</summary>
internal static unsafe class DirectWrite
{
    private static readonly Lazy<nint> Factory = new(() =>
    {
        Com.Check(Native.DWriteCreateFactory(Native.DWRITE_FACTORY_TYPE_SHARED, Native.IID_IDWriteFactory, out nint factory));
        return factory;
    });

    public static nint CreateTextFormat(string family, bool bold, bool italic, float sizeInDips)
    {
        nint factory = Factory.Value;
        nint format;
        string locale = CultureInfo.CurrentUICulture.Name is { Length: > 0 } name ? name : "en-us";

        fixed (char* familyPointer = family)
        fixed (char* localePointer = locale)
        {
            Com.Check(((delegate* unmanaged[Stdcall]<nint, char*, nint, int, int, int, float, char*, nint*, int>)
                Com.Method(factory, Slots.IDWriteFactory_CreateTextFormat))(
                    factory,
                    familyPointer,
                    0,
                    bold ? Native.DWRITE_FONT_WEIGHT_BOLD : Native.DWRITE_FONT_WEIGHT_NORMAL,
                    italic ? Native.DWRITE_FONT_STYLE_ITALIC : Native.DWRITE_FONT_STYLE_NORMAL,
                    Native.DWRITE_FONT_STRETCH_NORMAL,
                    sizeInDips,
                    localePointer,
                    &format));
        }

        return format;
    }

    public static nint CreateTextLayout(string text, nint format, float maxWidth, float maxHeight)
    {
        nint factory = Factory.Value;
        nint layout;

        fixed (char* textPointer = text)
        {
            Com.Check(((delegate* unmanaged[Stdcall]<nint, char*, uint, nint, float, float, nint*, int>)
                Com.Method(factory, Slots.IDWriteFactory_CreateTextLayout))(
                    factory, textPointer, (uint)text.Length, format, maxWidth, maxHeight, &layout));
        }

        return layout;
    }

    public static void SetTextAlignment(nint layout, int alignment) =>
        Com.Check(((delegate* unmanaged[Stdcall]<nint, int, int>)
            Com.Method(layout, Slots.IDWriteTextLayout_SetTextAlignment))(layout, alignment));

    public static void SetParagraphAlignment(nint layout, int alignment) =>
        Com.Check(((delegate* unmanaged[Stdcall]<nint, int, int>)
            Com.Method(layout, Slots.IDWriteTextLayout_SetParagraphAlignment))(layout, alignment));

    public static void SetWordWrapping(nint layout, int wrapping) =>
        Com.Check(((delegate* unmanaged[Stdcall]<nint, int, int>)
            Com.Method(layout, Slots.IDWriteTextLayout_SetWordWrapping))(layout, wrapping));

    public static void SetUnderline(nint layout, bool underline, Native.TextRange range) =>
        Com.Check(((delegate* unmanaged[Stdcall]<nint, int, Native.TextRange, int>)
            Com.Method(layout, Slots.IDWriteTextLayout_SetUnderline))(layout, underline ? 1 : 0, range));

    public static void SetStrikethrough(nint layout, bool strikethrough, Native.TextRange range) =>
        Com.Check(((delegate* unmanaged[Stdcall]<nint, int, Native.TextRange, int>)
            Com.Method(layout, Slots.IDWriteTextLayout_SetStrikethrough))(layout, strikethrough ? 1 : 0, range));

    public static Native.TextMetrics GetMetrics(nint layout)
    {
        Native.TextMetrics metrics;
        Com.Check(((delegate* unmanaged[Stdcall]<nint, Native.TextMetrics*, int>)
            Com.Method(layout, Slots.IDWriteTextLayout_GetMetrics))(layout, &metrics));
        return metrics;
    }
}

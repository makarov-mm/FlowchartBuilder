using System.Runtime.InteropServices;

namespace Makarov.FlowchartBuilder;

internal static class GlyphClipboard
{
    internal const string Format = "FlowchartBuilder.Selection.Xml";
    private const string DocumentStart = "<fb:FlowchartBuilderDocument";

    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint RegisterClipboardFormat(string name);

    [DllImport("user32.dll")]
    private static extern bool IsClipboardFormatAvailable(uint format);

    private static readonly uint NativeFormat = RegisterClipboardFormat(Format);
    private const uint UnicodeText = 13;
    private static uint? _sequence;
    private static bool _available;

    internal static bool Available
    {
        get
        {
            uint sequence = GetClipboardSequenceNumber();
            if (_sequence == sequence) return _available;
            try
            {
                _available = IsClipboardFormatAvailable(NativeFormat) || IsClipboardFormatAvailable(UnicodeText) &&
                    Clipboard.GetText().TrimStart().StartsWith(DocumentStart, StringComparison.Ordinal);
                _sequence = sequence;
                return _available;
            }
            catch (ExternalException) { return false; }
            catch (ThreadStateException) { return false; }
        }
    }

    internal static bool Copy()
    {
        if (Core.Instance.CurrentDocument.SelectionXml() is not { } xml) return false;
        try
        {
            Clipboard.SetDataObject(CreateData(xml), true);
            return true;
        }
        catch (ExternalException e) { Core.Instance.ShowError(e.Message); return false; }
    }

    internal static DataObject CreateData(string xml)
    {
        var data = new DataObject();
        data.SetData(Format, false, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml)));
        data.SetText(xml);
        return data;
    }

    internal static string Read(IDataObject? data)
    {
        if (data == null) return string.Empty;
        if (data.TryGetData<string>(DataFormats.UnicodeText, out var text)) return text;
        if (data.TryGetData<MemoryStream>(Format, out var stream))
            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        return string.Empty;
    }
}

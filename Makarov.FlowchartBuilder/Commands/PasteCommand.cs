using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands;

public sealed class PasteCommand : Command
{
    private string? _previousXml;
    private int _documentId;
    private float _offset;

    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet || !GlyphClipboard.Available) return;
        try
        {
            string xml = GlyphClipboard.Read(Clipboard.GetDataObject());
            var fragment = Document.ReadClipboard(xml);
            if (!fragment.HasGlyphs) return;
            int id = Core.Instance.CurrentDocument.Id;
            _offset = id == _documentId && xml == _previousXml ? _offset + 5f : 5f;
            _documentId = id;
            _previousXml = xml;
            sheet.DeselectAllGlyphs();
            foreach (var glyph in fragment.Glyphs.Reverse())
            {
                if (glyph is BlockGlyph block) { block.XInMM += _offset; block.YInMM += _offset; }
                glyph.Fixed = false;
                glyph.Selected = true;
                sheet.Add(glyph);
            }
            sheet.Groups.AddRange(fragment.Groups);
            sheet.UpdateGlyphsCommands();
            sheet.ShowProperties();
            Core.Instance.Redraw();
        }
        catch (Exception e) when (e is ExternalException or XmlException or InvalidDataException or
                                   ArgumentException or FormatException or OverflowException or TargetInvocationException)
        {
            Core.Instance.ShowError(Core.Instance.CurrentTranslator["Msg_InvalidClipboard"]);
        }
    }
}

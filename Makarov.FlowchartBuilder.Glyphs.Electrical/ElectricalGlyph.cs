using System.ComponentModel;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.Electrical;

[GlyphFamily("Electrical")]
[GlyphDefaultWidth(34)]
[GlyphDefaultHeight(20)]
public abstract class ElectricalGlyph : BlockGlyph
{
    private static readonly ConnectionPort[] TwoTerminals = [ConnectionPort.Left, ConnectionPort.Right];
    protected override IReadOnlyList<ConnectionPort> Ports => TwoTerminals;
    public override bool SupportsLinks => true;
    public override Type DefaultConnectorType => typeof(WireGlyph);

    protected ElectricalGlyph()
    {
        Caption = string.Empty;
        BorderWidthInMM = 0.4f;
    }

    protected abstract string ReferencePrefix { get; }

    [ActiveProperty, Category("Layout"), DisplayName("Component value")]
    public string Value { get; set; } = string.Empty;

    public override bool PreparePlacement(IWin32Window owner)
    {
        if (ReferencePrefix.Length != 0 && Core.Instance.ActiveSheet is { } sheet)
        {
            var references = sheet.Glyphs.Select(g => g.Caption).ToHashSet(StringComparer.OrdinalIgnoreCase);
            int number = 1;
            while (references.Contains(ReferencePrefix + number)) number++;
            Caption = ReferencePrefix + number;
        }
        return true;
    }

    // The same symbol geometry is used by the palette, editor, export and print.
    protected abstract void DrawSymbol(SymbolDrawing symbol);

    protected override void DrawGlyph(Canvas canvas)
    {
        var box = ScaledBounds;
        DrawSymbol(new SymbolDrawing(canvas, new(ScaledX, ScaledY), ScaledWidth / 2, ScaledHeight / 2, BorderColor, ScaledBorderWidth));
        var font = TextStyle.FromFont(TextFont).Scale(ScaleFactor);
        float labelHeight = box.Height * 0.26f;
        canvas.DrawText(Caption, font, new RectangleF(box.Left, box.Top, box.Width, labelHeight), ForegroundColor,
            TextAlignment.Center, ParagraphAlignment.Center);
        canvas.DrawText(Value, font, new RectangleF(box.Left, box.Bottom - labelHeight, box.Width, labelHeight), ForegroundColor,
            TextAlignment.Center, ParagraphAlignment.Center);
    }

    protected override void DrawText(Canvas canvas) { }

    protected override void DrawThumbnail(Canvas canvas)
    {
        base.DrawThumbnail(canvas);
        DrawSymbol(new SymbolDrawing(canvas, new(ThumbnailWidth / 2f, ThumbnailHeight / 2f),
            ThumbnailWidth / 2f - 4, ThumbnailHeight * 0.30f, Settings.Colors.ThumbnailBorder, 1.5f));
        DrawThumbnailBorder(canvas);
    }
}

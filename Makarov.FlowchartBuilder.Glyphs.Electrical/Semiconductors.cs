using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Electrical;

[GlyphName("Diode"), GlyphOrder(4)]
public sealed class DiodeGlyph : ElectricalGlyph
{
    protected override string ReferencePrefix => "D";
    protected override void DrawSymbol(SymbolDrawing s)
    {
        s.Leads(-6, 5);
        s.Triangle(-6, -6, -6, 6, 5, 0);
        s.Line(5, -6, 5, 6);
    }
}

[GlyphName("NPN transistor"), GlyphOrder(12)]
public sealed class NpnGlyph : ElectricalGlyph
{
    // Base, collector, emitter. The order is also used in saved wire endpoints.
    private static readonly ConnectionPort[] Terminals =
        [ConnectionPort.Left, new(new(0.55f, -1), new(0, -1)), new(new(0.55f, 1), new(0, 1))];
    protected override IReadOnlyList<ConnectionPort> Ports => Terminals;
    protected override string ReferencePrefix => "Q";
    protected override void DrawSymbol(SymbolDrawing s)
    {
        s.Line(-20, 0, -3, 0);
        s.Line(-3, -5, -3, 5);
        s.Line(-3, -2, 11, -8);
        s.Line(11, -8, 11, -12);
        s.Line(-3, 2, 11, 8);
        s.Triangle(11, 8, 5, 7.8f, 7, 3.8f);
        s.Line(11, 8, 11, 12);
    }
}

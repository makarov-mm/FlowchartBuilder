using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Electrical;

[GlyphName("Resistor"), GlyphOrder(1)]
public sealed class ResistorGlyph : ElectricalGlyph
{
    protected override string ReferencePrefix => "R";
    protected override void DrawSymbol(SymbolDrawing s)
    {
        s.Leads(-9, 9);
        s.Rectangle(-9, -3, 18, 6);
    }
}

[GlyphName("Capacitor"), GlyphOrder(2)]
public sealed class CapacitorGlyph : ElectricalGlyph
{
    protected override string ReferencePrefix => "C";
    protected override void DrawSymbol(SymbolDrawing s)
    {
        s.Leads(-2, 2);
        s.Line(-2, -6, -2, 6);
        s.Line(2, -6, 2, 6);
    }
}

[GlyphName("Inductor"), GlyphOrder(3)]
public sealed class InductorGlyph : ElectricalGlyph
{
    protected override string ReferencePrefix => "L";
    protected override void DrawSymbol(SymbolDrawing s)
    {
        s.Leads(-12, 12);
        for (int x = -12; x < 12; x += 6)
            s.Curve(x, 0, x, -6, x + 6, -6, x + 6, 0);
    }
}

[GlyphName("Fuse"), GlyphOrder(11)]
public sealed class FuseGlyph : ElectricalGlyph
{
    protected override string ReferencePrefix => "F";
    protected override void DrawSymbol(SymbolDrawing s)
    {
        s.Line(-20, 0, 20, 0);
        s.Rectangle(-9, -3, 18, 6);
    }
}

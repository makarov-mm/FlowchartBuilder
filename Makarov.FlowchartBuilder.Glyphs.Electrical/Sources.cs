using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Electrical;

public abstract class VoltageSourceGlyph : ElectricalGlyph
{
    protected override string ReferencePrefix => "V";
    protected override void DrawSymbol(SymbolDrawing s)
    {
        s.Leads(-7, 7);
        s.Ellipse(-7, -7, 14, 14);
        DrawMark(s);
    }

    protected abstract void DrawMark(SymbolDrawing s);
}

[GlyphName("DC voltage source"), GlyphOrder(6)]
public sealed class DcSourceGlyph : VoltageSourceGlyph
{
    protected override void DrawMark(SymbolDrawing s)
    {
        s.Line(-5, 0, -1, 0);
        s.Line(-3, -2, -3, 2);
        s.Line(1, 0, 5, 0);
    }
}

[GlyphName("AC voltage source"), GlyphOrder(7)]
public sealed class AcSourceGlyph : VoltageSourceGlyph
{
    protected override void DrawMark(SymbolDrawing s)
    {
        s.Curve(-5, 0, -3, -5, -1, -4, 0, 0);
        s.Curve(0, 0, 1, 4, 3, 5, 5, 0);
    }
}

[GlyphName("Battery"), GlyphOrder(8)]
public sealed class BatteryGlyph : ElectricalGlyph
{
    protected override string ReferencePrefix => "BT";
    protected override void DrawSymbol(SymbolDrawing s)
    {
        s.Leads(-5, 5);
        s.Line(-5, -6, -5, 6);
        s.Line(-2, -3, -2, 3);
        s.Line(2, -6, 2, 6);
        s.Line(5, -3, 5, 3);
    }
}

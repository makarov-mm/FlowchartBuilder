using System.ComponentModel;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Electrical;

[GlyphName("Ground"), GlyphOrder(5)]
public sealed class GroundGlyph : ElectricalGlyph
{
    private static readonly ConnectionPort[] Terminals = [ConnectionPort.Top];
    protected override IReadOnlyList<ConnectionPort> Ports => Terminals;
    protected override string ReferencePrefix => string.Empty;
    protected override void DrawSymbol(SymbolDrawing s)
    {
        s.Line(0, -12, 0, 0);
        s.Line(-8, 0, 8, 0);
        s.Line(-5, 3, 5, 3);
        s.Line(-2, 6, 2, 6);
    }
}

[GlyphName("Lamp"), GlyphOrder(9)]
public sealed class LampGlyph : ElectricalGlyph
{
    protected override string ReferencePrefix => "E";
    protected override void DrawSymbol(SymbolDrawing s)
    {
        s.Leads(-7, 7);
        s.Ellipse(-7, -7, 14, 14);
        s.Line(-5, -5, 5, 5);
        s.Line(-5, 5, 5, -5);
    }
}

[GlyphName("Switch"), GlyphOrder(10)]
public sealed class SwitchGlyph : ElectricalGlyph
{
    protected override string ReferencePrefix => "S";

    [ActiveProperty, Category("Appearance"), DisplayName("Contacts closed")]
    public bool Closed { get; set; }

    protected override void DrawSymbol(SymbolDrawing s)
    {
        s.Leads(-9, 9);
        s.Ellipse(-9, -1, 2, 2);
        s.Ellipse(7, -1, 2, 2);
        s.Line(-7, 0, 7, Closed ? 0 : -6);
    }
}

[GlyphName("Wire junction"), GlyphOrder(13)]
[GlyphDefaultWidth(5), GlyphDefaultHeight(5), GlyphMinimalWidth(3), GlyphMinimalHeight(3)]
public sealed class JunctionGlyph : ElectricalGlyph
{
    private static readonly ConnectionPort[] Terminals =
        [ConnectionPort.Top, ConnectionPort.Right, ConnectionPort.Bottom, ConnectionPort.Left];
    protected override IReadOnlyList<ConnectionPort> Ports => Terminals;
    public override bool CanRotate => false;
    protected override string ReferencePrefix => string.Empty;
    protected override void DrawSymbol(SymbolDrawing s)
    {
        s.Line(-20, 0, 20, 0);
        s.Line(0, -12, 0, 12);
        s.Dot(5, 3);
    }
}

[GlyphFamily("Electrical"), GlyphName("Wire"), GlyphOrder(14)]
public sealed class WireGlyph : ConnectorGlyph
{
    public WireGlyph()
    {
        Routing = ConnectorRouting.Orthogonal;
        ArrowHead = false;
        LineWidthInMM = 0.35f;
    }
}

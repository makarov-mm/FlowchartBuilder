using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Simple;

/// <summary>Smooth curved arrow.</summary>
[GlyphName("Arrow")]
[GlyphOrder(1)]
public sealed class ArrowGlyph : AbstractLinkGlyph
{
    public ArrowGlyph()
    {
        Routing = ConnectorRouting.Curved;
        ArrowHead = true;
    }
}

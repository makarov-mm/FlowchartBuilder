using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Simple;

/// <summary>Arrow made of horizontal and vertical segments.</summary>
[GlyphName("Broken Arrow")]
[GlyphOrder(2)]
public sealed class BrokenArrowGlyph : AbstractLinkGlyph
{
    public BrokenArrowGlyph()
    {
        Routing = ConnectorRouting.Orthogonal;
        ArrowHead = true;
    }
}

using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Simple;

/// <summary>Straight line without an arrowhead.</summary>
[GlyphName("Link")]
[GlyphOrder(3)]
public sealed class LinkGlyph : AbstractLinkGlyph
{
    public LinkGlyph()
    {
        Routing = ConnectorRouting.Straight;
        ArrowHead = false;
    }
}

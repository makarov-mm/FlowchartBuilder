using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Simple;

/// <summary>
/// Link tools of the palette: pick one, then drag from a flowchart block to another to connect them with a
/// connector of this style. Routing and arrowhead can still be changed in the Properties window afterwards.
/// </summary>
[GlyphFamily("Links")]
public abstract class AbstractLinkGlyph : ConnectorGlyph;

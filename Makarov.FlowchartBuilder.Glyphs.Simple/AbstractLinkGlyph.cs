using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Simple;

/// <summary>
/// Palette entries for links. They have no drawing of their own yet and appear on the sheet as
/// placeholder blocks; connectors are created by dragging from a link point of a flowchart block.
/// </summary>
[GlyphFamily("Links")]
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(30)]
public abstract class AbstractLinkGlyph : BlockGlyph
{
    protected AbstractLinkGlyph()
    {
        BackgroundColor = Color.Silver;
        ForegroundColor = Color.WhiteSmoke;
    }

    protected override void DrawText(Graphics graphics)
    {
    }
}

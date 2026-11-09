using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.Simple;

/// <summary>
/// Common glyphs (text, image). They have no drawing of their own yet and are hidden from the palette.
/// </summary>
[GlyphFamily("Common")]
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(30)]
public abstract class AbstractSimpleGlyph : BlockGlyph
{
    protected AbstractSimpleGlyph()
    {
        BackgroundColor = Color.Silver;
        ForegroundColor = Color.WhiteSmoke;
    }

    protected override void DrawText(Canvas canvas)
    {
    }
}

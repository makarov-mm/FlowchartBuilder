using System.Drawing;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Simple
{

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

        protected override void DrawText(Graphics gfx)
        {

        }
    }
}

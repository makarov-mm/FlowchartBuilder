using System.Drawing;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs.Simple
{

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

        protected override void DrawText(Graphics gfx)
        {

        }
    }
}

using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.Simple;

[GlyphName("Text")]
public sealed class TextGlyph : AbstractSimpleGlyph
{
    protected override string ThumbnailLabel => "Aa";

    protected override void DrawGlyph(Canvas canvas)
    {
        var style = TextStyle.FromFont(TextFont).Scale(ScaleFactor);
        using (canvas.Translate(ScaleFactor, ScaleFactor))
            canvas.DrawText(Caption, style, ScaledBounds, Color.FromArgb(48, Color.Black));
        canvas.DrawText(Caption, style, ScaledBounds, ForegroundColor);
    }
}

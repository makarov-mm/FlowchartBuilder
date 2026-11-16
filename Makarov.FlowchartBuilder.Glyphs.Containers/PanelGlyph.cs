using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.Containers;

[GlyphName("Panel")]
public sealed class PanelGlyph : AbstractContainerGlyph
{
    protected override void DrawThumbnail(Canvas canvas)
    {
        DrawThumbnailFrame(canvas);
        canvas.DrawRectangle(ThumbnailFrame, Settings.Strokes.ThumbnailBorder);
    }
}

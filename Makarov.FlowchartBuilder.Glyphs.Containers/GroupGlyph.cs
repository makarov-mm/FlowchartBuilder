using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.Containers;

[GlyphName("Group")]
public sealed class GroupGlyph : AbstractContainerGlyph
{
    private static readonly Stroke ThumbnailFrameStroke = new(Settings.Colors.ThumbnailBorder) { Dash = new DashPattern(3f, 1f) };

    protected override void DrawThumbnail(Canvas canvas)
    {
        DrawThumbnailFrame(canvas);
        canvas.DrawRectangle(ThumbnailFrame, ThumbnailFrameStroke);
    }
}

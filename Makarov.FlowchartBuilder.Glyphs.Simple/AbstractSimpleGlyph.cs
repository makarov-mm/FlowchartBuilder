using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.Simple;

[GlyphFamily("Common")]
[GlyphDefaultWidth(40)]
[GlyphDefaultHeight(22)]
public abstract class AbstractSimpleGlyph : BlockGlyph
{
    protected override void DrawText(Canvas canvas) { }

    protected override void DrawThumbnail(Canvas canvas)
    {
        base.DrawThumbnail(canvas);
        var rect = new RectangleF(5, 8, ThumbnailWidth - 10, ThumbnailHeight - 16);
        canvas.FillRectangle(rect, new LinearGradient(rect.Location, new PointF(rect.Right, rect.Bottom),
            Settings.Colors.ThumbnailObjectGradientStart, Settings.Colors.ThumbnailObjectGradientEnd));
        canvas.DrawText(ThumbnailLabel, new TextStyle("Segoe UI", 16), rect, Settings.Colors.ThumbnailBorder,
            TextAlignment.Center, ParagraphAlignment.Center);
        DrawThumbnailBorder(canvas);
    }

    protected abstract string ThumbnailLabel { get; }
}

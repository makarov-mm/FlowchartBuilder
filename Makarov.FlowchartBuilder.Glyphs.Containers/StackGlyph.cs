using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.Containers;

[GlyphName("Stack")]
public sealed class StackGlyph : AbstractContainerGlyph
{
    /// <summary>The palette image shows a stack with room for five items, three of them filled.</summary>
    private const int Slots = 5;
    private const int FilledSlots = 3;
    private const int Spacing = 2;

    protected override void DrawThumbnail(Canvas canvas)
    {
        DrawThumbnailFrame(canvas);

        Rectangle frame = ThumbnailFrame;
        canvas.DrawRectangle(frame, Settings.Strokes.ThumbnailBorder);

        int cellWidth = frame.Width - (Spacing << 1);
        int cellHeight = (frame.Height - (Slots + 1) * Spacing) / Slots;

        for (int row = 0; row < FilledSlots; row++)
        {
            DrawThumbnailCell(
                canvas,
                new Rectangle(
                    frame.Left + Spacing,
                    frame.Top + (row + 1) * Spacing + row * cellHeight,
                    cellWidth,
                    cellHeight));
        }
    }
}

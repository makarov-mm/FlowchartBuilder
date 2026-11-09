using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.Containers;

[GlyphName("Table")]
public sealed class TableGlyph : AbstractContainerGlyph
{
    /// <summary>The palette image shows a 3x3 table with the last two cells still empty.</summary>
    private const int Columns = 3;
    private const int Rows = 3;
    private const int FilledCells = 7;
    private const int Spacing = 2;

    protected override void DrawThumbnail(Canvas canvas)
    {
        DrawThumbnailFrame(canvas);

        Rectangle frame = ThumbnailFrame;
        canvas.DrawRectangle(frame, Settings.Strokes.ThumbnailBorder);

        int cellWidth = (frame.Width - (Columns + 1) * Spacing) / Columns;
        int cellHeight = (frame.Height - (Rows + 1) * Spacing) / Rows;

        for (int index = 0; index < FilledCells; index++)
        {
            int column = index % Columns;
            int row = index / Columns;

            DrawThumbnailCell(
                canvas,
                new Rectangle(
                    frame.Left + (column + 1) * Spacing + column * cellWidth,
                    frame.Top + (row + 1) * Spacing + row * cellHeight,
                    cellWidth,
                    cellHeight));
        }
    }
}

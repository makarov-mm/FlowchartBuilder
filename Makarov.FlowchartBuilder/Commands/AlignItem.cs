using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Sheets;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>
/// One unit for alignment: a single selected glyph, or a whole group with at least one selected member.
/// Coordinates are in millimeters.
/// </summary>
public sealed class AlignItem
{
    private AlignItem(IReadOnlyList<BlockGlyph> glyphs)
    {
        Glyphs = glyphs;
    }

    public IReadOnlyList<BlockGlyph> Glyphs { get; }

    public RectangleF Bounds => GetBounds(Glyphs);

    public static List<AlignItem> FromSelection(Sheet sheet)
    {
        var items = new List<AlignItem>();
        var seenGroups = new HashSet<List<BlockGlyph>>();

        foreach (BlockGlyph glyph in sheet.SelectedBlockGlyphs)
        {
            if (glyph.Group is not { } group)
                items.Add(new AlignItem([glyph]));
            else if (seenGroups.Add(group))
                items.Add(new AlignItem(group));
        }

        return items;
    }

    public static RectangleF GetBounds(IEnumerable<BlockGlyph> glyphs) =>
        glyphs.Select(glyph => glyph.BoundsInMM).Aggregate(RectangleF.Union);

    public void MoveBy(float dx, float dy)
    {
        foreach (BlockGlyph glyph in Glyphs)
        {
            glyph.XInMM += dx;
            glyph.YInMM += dy;
        }
    }
}

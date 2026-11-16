namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Puts the vertical centers on the average center of the selected glyphs.</summary>
public sealed class AlignCenterVerticalCommand : AlignCommand
{
    protected override void Align(IReadOnlyList<AlignItem> items)
    {
        float center = items.SelectMany(item => item.Glyphs).Average(glyph => glyph.YInMM);
        foreach (AlignItem item in items)
            item.MoveBy(0, center - (item.Bounds.Top + item.Bounds.Bottom) / 2);
    }
}

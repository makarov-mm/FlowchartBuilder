namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Puts the horizontal centers on the average center of the selected glyphs.</summary>
public sealed class AlignCenterHorizontalCommand : AlignCommand
{
    protected override void Align(IReadOnlyList<AlignItem> items)
    {
        float center = items.SelectMany(item => item.Glyphs).Average(glyph => glyph.XInMM);
        foreach (AlignItem item in items)
            item.MoveBy(center - (item.Bounds.Left + item.Bounds.Right) / 2, 0);
    }
}

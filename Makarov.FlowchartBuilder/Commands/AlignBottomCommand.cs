namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Lines up the bottom edges with the lowest one.</summary>
public sealed class AlignBottomCommand : AlignCommand
{
    protected override void Align(IReadOnlyList<AlignItem> items)
    {
        float bottom = items.Max(item => item.Bounds.Bottom);
        foreach (AlignItem item in items)
            item.MoveBy(0, bottom - item.Bounds.Bottom);
    }
}

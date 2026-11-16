namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Lines up the left edges with the leftmost one.</summary>
public sealed class AlignLeftCommand : AlignCommand
{
    protected override void Align(IReadOnlyList<AlignItem> items)
    {
        float left = items.Min(item => item.Bounds.Left);
        foreach (AlignItem item in items)
            item.MoveBy(left - item.Bounds.Left, 0);
    }
}

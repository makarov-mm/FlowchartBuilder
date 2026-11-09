namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Lines up the right edges with the rightmost one.</summary>
public sealed class AlignRightCommand : AlignCommand
{
    protected override void Align(IReadOnlyList<AlignItem> items)
    {
        float right = items.Max(item => item.Bounds.Right);
        foreach (AlignItem item in items)
            item.MoveBy(right - item.Bounds.Right, 0);
    }
}

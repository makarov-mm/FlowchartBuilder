namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Lines up the top edges with the topmost one.</summary>
public sealed class AlignTopCommand : AlignCommand
{
    protected override void Align(IReadOnlyList<AlignItem> items)
    {
        float top = items.Min(item => item.Bounds.Top);
        foreach (AlignItem item in items)
            item.MoveBy(0, top - item.Bounds.Top);
    }
}

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>
/// Aligns the selected glyphs relative to each other; a group is aligned as a whole.
/// </summary>
public abstract class AlignCommand : Command
{
    public sealed override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        List<AlignItem> items = AlignItem.FromSelection(sheet);
        if (items.Count == 0)
            return;

        Align(items);
        Core.Instance.Redraw();
    }

    protected abstract void Align(IReadOnlyList<AlignItem> items);
}

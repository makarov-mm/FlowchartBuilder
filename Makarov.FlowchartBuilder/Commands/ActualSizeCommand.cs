namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Restores 100% zoom.</summary>
public sealed class ActualSizeCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        sheet.ScaleFactor = 1f;

        Core.Instance.Redraw();
    }
}

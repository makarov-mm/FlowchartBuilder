namespace Makarov.FlowchartBuilder.Commands;

public sealed class ZoomInCommand : Command
{
    private const float Step = 1.1f;

    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        sheet.ScaleFactor *= Step;
        Core.Instance.Redraw();
    }
}

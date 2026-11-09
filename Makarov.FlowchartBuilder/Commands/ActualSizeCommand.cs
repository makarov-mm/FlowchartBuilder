using Makarov.FlowchartBuilder.Sheets;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Zooms so that the whole paper fits into 90% of the work area.</summary>
public sealed class ActualSizeCommand : Command
{
    private const float WindowFraction = 0.9f;

    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        sheet.ScaleFactor = sheet is FixedSheet fixedSheet
            ? fixedSheet.GetFitScaleFactor(Core.Instance.MainWindow.InnerRectangle.Size, WindowFraction)
            : 1f;

        Core.Instance.Redraw();
    }
}

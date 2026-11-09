using Makarov.FlowchartBuilder.Sheets;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Zooms so that the paper fills the work area.</summary>
public sealed class ZoomToWindowCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        sheet.ScaleFactor = sheet is FixedSheet fixedSheet
            ? fixedSheet.GetFitScaleFactor(Core.Instance.MainWindow.InnerRectangle.Size, 1f)
            : 1f;

        Core.Instance.Redraw();
    }
}

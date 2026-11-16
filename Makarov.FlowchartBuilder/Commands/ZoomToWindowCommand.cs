namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Zooms so that the diagram fits the work area.</summary>
public sealed class ZoomToWindowCommand : Command
{
    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        Size viewport = Core.Instance.MainWindow.InnerRectangle.Size;
        Rectangle bounds = sheet.GetContentBounds();
        if (bounds.IsEmpty)
        {
            sheet.ScaleFactor = 1f;
            sheet.ViewOrigin = PointF.Empty;
        }
        else
        {
            sheet.ScaleFactor = Math.Min((float)viewport.Width / bounds.Width, (float)viewport.Height / bounds.Height);
            sheet.ViewOrigin = new PointF(
                bounds.Left + (bounds.Width - viewport.Width / sheet.ScaleFactor) / 2,
                bounds.Top + (bounds.Height - viewport.Height / sheet.ScaleFactor) / 2);
        }

        Core.Instance.Redraw();
    }
}

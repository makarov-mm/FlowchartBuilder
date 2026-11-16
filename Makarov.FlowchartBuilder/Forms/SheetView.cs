using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Forms;

/// <summary>Drawing area of the main window: the current sheet and the rulers, painted with Direct2D.</summary>
public sealed class SheetView : CanvasControl
{
    protected override void OnRender(CanvasRenderEventArgs e)
    {
        base.OnRender(e);
        Core.Instance.Paint(e.Canvas);
    }
}

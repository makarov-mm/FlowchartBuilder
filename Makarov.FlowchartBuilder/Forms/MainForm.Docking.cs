using Makarov.FlowchartBuilder.Commands;

namespace Makarov.FlowchartBuilder.Forms;

public partial class MainForm
{
    private readonly List<DockingForm> _toolWindows = [];
    private bool _arrangingTools;

    private void RestoreTools()
    {
        Register(Core.Instance.Windows.GetWindow<GlyphsForm>(), DockStyle.Left, Command.GetInstance<PaletteWindowCommand>());
        Register(Core.Instance.Windows.GetWindow<PropertiesForm>(), DockStyle.Right, Command.GetInstance<PropertiesWindowCommand>());

        void Register(DockingForm tool, DockStyle side, Command command)
        {
            _toolWindows.Add(tool);
            tool.RestorePlacement(this, side, command);
        }
    }

    internal void ResetTools()
    {
        foreach (var tool in _toolWindows)
            tool.ResetPlacement();
    }

    internal DockStyle DropSide(Point screen)
    {
        Rectangle area = RectangleToScreen(ViewBounds);
        int range = LogicalToDeviceUnits(44);
        if (!Rectangle.Inflate(area, range / 3, range / 3).Contains(screen))
            return DockStyle.None;

        int left = Math.Abs(screen.X - area.Left), right = Math.Abs(screen.X - area.Right);
        int top = Math.Abs(screen.Y - area.Top), bottom = Math.Abs(screen.Y - area.Bottom);
        int nearest = Math.Min(Math.Min(left, right), Math.Min(top, bottom));
        if (nearest > range) return DockStyle.None;
        if (nearest == left) return DockStyle.Left;
        if (nearest == right) return DockStyle.Right;
        return nearest == top ? DockStyle.Top : DockStyle.Bottom;
    }

    // Preview and layout use the same allocation, including when both tools choose one edge.
    internal Rectangle ArrangeTools(DockingForm? preview = null, DockStyle proposedSide = DockStyle.None)
    {
        Rectangle area = ViewBounds;
        Rectangle proposed = Rectangle.Empty;
        if (_arrangingTools) return proposed;
        _arrangingTools = true;
        try
        {
            foreach (DockingForm tool in _toolWindows)
            {
                DockStyle side = tool == preview ? proposedSide : tool.Side;
                if (!tool.IsOpen || side == DockStyle.None) continue;

                bool vertical = side is DockStyle.Left or DockStyle.Right;
                int available = vertical ? area.Width : area.Height;
                int wanted = vertical ? tool.DockWidth : tool.DockHeight;
                // Leave room for the diagram even when restoring onto a smaller monitor.
                int extent = Math.Min(wanted, Math.Max(0, available / 2));
                Rectangle slot = area;
                if (vertical)
                {
                    slot.Width = extent;
                    if (side == DockStyle.Right) slot.X = area.Right - extent;
                    else area.X += extent;
                    area.Width -= extent;
                }
                else
                {
                    slot.Height = extent;
                    if (side == DockStyle.Bottom) slot.Y = area.Bottom - extent;
                    else area.Y += extent;
                    area.Height -= extent;
                }

                if (preview is null) tool.Bounds = slot;
                else if (preview == tool) proposed = RectangleToScreen(slot);
            }
            if (preview is null) sheetView.Bounds = area;
            return proposed;
        }
        finally
        {
            _arrangingTools = false;
        }
    }
}

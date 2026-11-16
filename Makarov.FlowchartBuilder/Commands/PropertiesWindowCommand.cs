using Makarov.FlowchartBuilder.Forms;

namespace Makarov.FlowchartBuilder.Commands;

public sealed class PropertiesWindowCommand : Command
{
    /// <summary>Shows the tool window again after the user closed (hid) it.</summary>
    public override void Run()
    {
        Enabled = false;

        var window = Core.Instance.GetWindowAsChild<PropertiesForm>();
        window.Visible = !window.Visible;
        Core.Instance.Redraw();
    }
}

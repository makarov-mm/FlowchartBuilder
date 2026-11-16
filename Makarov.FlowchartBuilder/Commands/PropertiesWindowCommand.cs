using Makarov.FlowchartBuilder.Forms;

namespace Makarov.FlowchartBuilder.Commands;

public sealed class PropertiesWindowCommand : Command
{
    /// <summary>Shows the tool window again after the user closed (hid) it.</summary>
    public override void Run() => Core.Instance.Windows.GetWindow<PropertiesForm>().ShowTool();
}

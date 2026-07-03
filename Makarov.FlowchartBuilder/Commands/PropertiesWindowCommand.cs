using Makarov.FlowchartBuilder.Forms;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class PropertiesWindowCommand : Command
    {

        public override void Run()
        {

            Enabled = false;

            var propWindow = Core.Instance.Windows.GetWindow<PropertiesForm>();
            propWindow.Visible = !propWindow.Visible;

            Core.Instance.Redraw();
        }
    }
}

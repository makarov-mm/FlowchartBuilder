using Makarov.FlowchartBuilder.Forms;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class AboutCommand : Command
    {

        public override void Run()
        {
            Core.Instance.Windows.GetWindow<AboutForm>().ShowDialog(Core.Instance.MainWindow);
        }
    }
}

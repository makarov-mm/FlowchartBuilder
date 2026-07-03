using Makarov.FlowchartBuilder.Forms;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class ShowSettingsCommand : Command
    {

        public override void Run()
        {
            Core.Instance.Windows.GetWindow<SettingsForm>().ShowDialog(Core.Instance.MainWindow);
        }
    }
}

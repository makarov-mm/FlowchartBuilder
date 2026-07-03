using Makarov.FlowchartBuilder.Forms;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class NewCommand : Command
    {

        public override void Run()
        {

            using (var form = new SheetsForm())
                form.ShowDialog(Core.Instance.MainWindow);
        }
    }
}

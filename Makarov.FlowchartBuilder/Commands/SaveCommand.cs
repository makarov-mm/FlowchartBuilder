using System.Windows.Forms;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class SaveCommand : Command
    {

        public override void Run()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "FlowchartBuilder Documents|*.mfb";

                if (dialog.ShowDialog(Core.Instance.MainWindow) == DialogResult.OK)
                {
                    Core.Instance.CurrentDocument.Save(dialog.FileName);
                }
            }
        }
    }
}

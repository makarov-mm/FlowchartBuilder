using System.Windows.Forms;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class OpenCommand : Command
    {

        public override void Run()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = @"FlowchartBuilder Documents|*.mfb";

                if (dialog.ShowDialog(Core.Instance.MainWindow) == DialogResult.OK)
                {
                    Core.Instance.AddDocument(new Document(dialog.FileName));
                    Core.Instance.Redraw();
                }
            }
        }
    }
}

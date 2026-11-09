using Makarov.FlowchartBuilder.Forms;

namespace Makarov.FlowchartBuilder.Commands;

public sealed class AboutCommand : Command
{
    public override void Run()
    {
        using var dialog = new AboutForm();
        dialog.ShowDialog(Core.Instance.MainWindow);
    }
}

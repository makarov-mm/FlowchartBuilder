using Makarov.FlowchartBuilder.Forms;

namespace Makarov.FlowchartBuilder.Commands;

public sealed class ShowSettingsCommand : Command
{
    public override void Run()
    {
        using var dialog = new SettingsForm();
        dialog.ShowDialog(Core.Instance.MainWindow);
    }
}

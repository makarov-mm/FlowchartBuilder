using Makarov.FlowchartBuilder.Forms;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Lets the user pick a paper format and opens a new document on it.</summary>
public sealed class NewCommand : Command
{
    public override void Run()
    {
        using var dialog = new SheetsForm();
        dialog.ShowDialog(Core.Instance.MainWindow);
    }
}

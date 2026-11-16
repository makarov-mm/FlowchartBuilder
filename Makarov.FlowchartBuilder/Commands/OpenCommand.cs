namespace Makarov.FlowchartBuilder.Commands;

public sealed class OpenCommand : Command
{
    public override void Run()
    {
        using var dialog = new OpenFileDialog { Filter = Settings.Environment.DocumentFilter };

        if (dialog.ShowDialog(Core.Instance.MainWindow) == DialogResult.OK)
            Core.Instance.TryOpenDocument(dialog.FileName);
    }
}

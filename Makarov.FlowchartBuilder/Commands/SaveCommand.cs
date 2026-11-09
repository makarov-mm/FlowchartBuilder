namespace Makarov.FlowchartBuilder.Commands;

public sealed class SaveCommand : Command
{
    public override void Run()
    {
        if (!Core.Instance.HasDocuments)
            return;

        Document document = Core.Instance.CurrentDocument;
        using var dialog = new SaveFileDialog
        {
            Filter = Settings.Environment.DocumentFilter,
            DefaultExt = Settings.Environment.DocumentExtension,
            FileName = document.FileName ?? document.Name ?? string.Empty,
        };

        if (dialog.ShowDialog(Core.Instance.MainWindow) != DialogResult.OK)
            return;

        try
        {
            document.Save(dialog.FileName);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Core.Instance.ShowError(e.Message);
        }
    }
}

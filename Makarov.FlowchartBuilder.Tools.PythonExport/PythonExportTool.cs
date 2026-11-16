namespace Makarov.FlowchartBuilder.Tools.PythonExport;

public sealed class PythonExportTool : IDocumentTool
{
    public string ExampleFileName => "Countdown.mfb";

    public string Title(string language) => language switch
    {
        "Russian" => "Блок-схема → Python…", "Ukrainian" => "Блок-схема → Python…", _ => "Flowchart → Python…"
    };

    public void Run(Document document, IWin32Window owner, string language)
    {
        string code = PythonGenerator.Generate(document);
        using var preview = new Form { Text = Title(language), Size = new Size(760, 560), StartPosition = FormStartPosition.CenterParent };
        var text = new TextBox { Multiline = true, ReadOnly = true, WordWrap = false, ScrollBars = ScrollBars.Both,
            Font = new Font("Consolas", 10), Dock = DockStyle.Fill, Text = code };
        var save = new Button { Text = language switch { "Russian" => "Сохранить .py…", "Ukrainian" => "Зберегти .py…", _ => "Save .py…" }, AutoSize = true };
        save.Click += (_, _) =>
        {
            using var dialog = new SaveFileDialog { Filter = "Python (*.py)|*.py", DefaultExt = "py", FileName = "flowchart.py" };
            if (dialog.ShowDialog(preview) != DialogResult.OK) return;
            try { File.WriteAllText(dialog.FileName, code); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { MessageBox.Show(preview, ex.Message, preview.Text); }
        };
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
        toolbar.Controls.Add(save);
        preview.Controls.Add(text);
        preview.Controls.Add(toolbar);
        preview.ShowDialog(owner);
    }
}

namespace Makarov.FlowchartBuilder.Tools.PythonExport;

internal sealed class PythonPreviewForm : Form
{
    private readonly RichTextBox _code = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, WordWrap = false, DetectUrls = false,
        HideSelection = false, BackColor = SystemColors.Window
    };
    private readonly Font _codeFont = new("Consolas", 10);
    private readonly Button _save = new() { AutoSize = true };
    private readonly string _language;
    private readonly string _title;
    private PythonGenerationResult? _result;
    private bool _synchronizing;

    public Document Document { get; }

    public PythonPreviewForm(Document document, string language, string title)
    {
        Document = document;
        _language = language;
        _title = title.TrimEnd('…');
        Size = new Size(560, 460);
        MinimumSize = new Size(340, 240);
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.SizableToolWindow;
        _code.Font = _codeFont;
        _save.Text = T("Save .py…", "Сохранить .py…", "Зберегти .py…");
        _save.Click += (_, _) => SaveCode();
        _code.SelectionChanged += (_, _) => NavigateFromCode();
        _code.MouseClick += (_, _) => NavigateFromCode();
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
        toolbar.Controls.Add(_save);
        Controls.Add(_code);
        Controls.Add(toolbar);
    }

    private string T(string en, string ru, string uk) => _language switch
    {
        "Russian" => ru, "Ukrainian" => uk, _ => en
    };

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (Owner is Form editor)
        {
            var screen = Screen.FromControl(editor).WorkingArea;
            var work = Core.Instance.MainWindow;
            var canvas = work.RectangleToScreen(work.InnerRectangle);
            Location = new Point(Math.Max(screen.Left, Math.Min(canvas.Right + 8, screen.Right - Width)),
                Math.Max(screen.Top, Math.Min(canvas.Top, screen.Bottom - Height)));
        }

        Document.Changed += DocumentChanged;
        Core.Instance.SelectionChanged += EditorSelectionChanged;
        Core.Instance.DocumentClosed += DocumentClosed;
        RefreshCode();
    }

    private void DocumentChanged(object? sender, EventArgs e) => RefreshCode();

    private void DocumentClosed(Document document)
    {
        if (Document == document) Close();
    }

    private void RefreshCode()
    {
        _synchronizing = true;
        try
        {
            Text = $"{_title} — {Document.DisplayName}";
            var graph = FlowchartValidator.Analyze(Document);
            if (graph.Diagnostics.FirstOrDefault() is { } problem)
            {
                _result = null;
                _code.Text = problem.Message(_language);
            }
            else
            {
                var updated = PythonGenerator.GenerateWithSourceMap(graph);
                if (_result?.Code != updated.Code) _code.Text = updated.Code;
                _result = updated;
            }
            _save.Enabled = _result != null;
        }
        finally { _synchronizing = false; }

        EditorSelectionChanged(this, EventArgs.Empty);
    }

    private void NavigateFromCode()
    {
        if (_synchronizing || _result == null || !_code.Focused) return;
        int first = _code.GetLineFromCharIndex(_code.SelectionStart);
        int last = _code.GetLineFromCharIndex(_code.SelectionStart + Math.Max(0, _code.SelectionLength - 1));
        if (first != last || !_result.LineToBlockId.TryGetValue(first + 1, out string? id)) return;

        _synchronizing = true;
        try { Core.Instance.RevealGlyph(Document, id); }
        finally { _synchronizing = false; }
    }

    private void EditorSelectionChanged(object? sender, EventArgs e)
    {
        if (_synchronizing || _result == null) return;
        var core = Core.Instance;
        if (!core.HasDocuments || core.CurrentDocument != Document) return;
        var selection = Document.DocumentSheet.SelectedGlyphs.Take(2).ToArray();
        int line = selection.Length == 1
            ? _result.LineToBlockId.FirstOrDefault(pair => pair.Value == selection[0].Id).Key
            : 0;
        _synchronizing = true;
        try
        {
            if (line == 0)
            {
                _code.DeselectAll();
                return;
            }
            int start = _code.GetFirstCharIndexFromLine(line - 1);
            if (start < 0) return;
            int end = _code.GetFirstCharIndexFromLine(line);
            int length = end < 0 ? _code.TextLength - start : end - start - 1;
            _code.Select(start, Math.Max(0, length));
            _code.ScrollToCaret();
        }
        finally { _synchronizing = false; }
    }

    private void SaveCode()
    {
        if (_result == null) return;
        using var dialog = new SaveFileDialog { Filter = "Python (*.py)|*.py", DefaultExt = "py", FileName = "flowchart.py" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { File.WriteAllText(dialog.FileName, _result.Code); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Document.Changed -= DocumentChanged;
            Core.Instance.SelectionChanged -= EditorSelectionChanged;
            Core.Instance.DocumentClosed -= DocumentClosed;
            _codeFont.Dispose();
        }
        base.Dispose(disposing);
    }
}

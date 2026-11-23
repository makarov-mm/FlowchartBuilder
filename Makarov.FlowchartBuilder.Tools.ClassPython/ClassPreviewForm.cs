namespace Makarov.FlowchartBuilder.Tools.ClassPython;

internal sealed class ClassPreviewForm : Form
{
    private readonly string _language;
    private readonly string _title;
    private readonly Font _codeFont = new("Consolas", 10);
    private readonly TextBox _code = new()
    {
        Multiline = true, ReadOnly = true, WordWrap = false, ScrollBars = ScrollBars.Both,
        BackColor = SystemColors.Window, Dock = DockStyle.Fill
    };
    private readonly DataGridView _issues = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, RowHeadersVisible = false,
        AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
    };
    private readonly Button _save = new() { AutoSize = true };
    public Document Document { get; }

    public ClassPreviewForm(Document document, string language, string title)
    {
        Document = document;
        _language = language;
        _title = title.TrimEnd('…');
        Size = new Size(640, 480);
        MinimumSize = new Size(360, 220);
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        _code.Font = _codeFont;
        _issues.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _issues.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = T("Element", "Элемент", "Елемент"), FillWeight = 30,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, SortMode = DataGridViewColumnSortMode.NotSortable
        });
        _issues.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = T("What to fix", "Что исправить", "Що виправити"), FillWeight = 70,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, SortMode = DataGridViewColumnSortMode.NotSortable
        });
        _issues.CellClick += (_, e) => { if (e.RowIndex >= 0) RevealIssue(); };
        _issues.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            RevealIssue();
        };
        _save.Text = T("Save .py…", "Сохранить .py…", "Зберегти .py…");
        _save.Click += (_, _) => SaveModule();
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
        toolbar.Controls.Add(_save);
        Controls.Add(_code);
        Controls.Add(_issues);
        Controls.Add(toolbar);
        Document.Changed += DocumentChanged;
        Core.Instance.DocumentClosed += DocumentClosed;
        RefreshPreview();
    }

    private string T(string en, string ru, string uk) => _language switch { "Russian" => ru, "Ukrainian" => uk, _ => en };

    private void DocumentChanged(object? sender, EventArgs e) => RefreshPreview();

    private void DocumentClosed(Document document)
    {
        if (document == Document) Close();
    }

    private void RefreshPreview()
    {
        Text = $"{_title} — {Document.DisplayName}";
        var result = ClassDiagramReader.Generate(Document);
        bool valid = result.Issues.Count == 0;
        _save.Enabled = valid;
        _code.Visible = valid;
        _issues.Visible = !valid;
        if (_code.Text != result.Code) _code.Text = result.Code;
        _issues.Rows.Clear();
        if (valid) return;

        var glyphs = Document.DocumentSheet.Glyphs.ToLookup(glyph => glyph.Id);
        foreach (var issue in result.Issues)
        {
            var glyph = issue.GlyphId is null ? null : glyphs[issue.GlyphId].FirstOrDefault();
            string label = glyph switch
            {
                Glyphs.ConnectorGlyph { Source: { } from, Target: { } to } => $"{from.Caption} → {to.Caption}",
                null => T("Document", "Документ", "Документ"),
                _ => string.IsNullOrWhiteSpace(glyph.Caption) ? Core.Instance.CurrentTranslator[glyph.Name] : glyph.Caption
            };
            int row = _issues.Rows.Add(label, issue.Message(_language));
            _issues.Rows[row].Tag = issue.GlyphId;
        }
    }

    private void RevealIssue()
    {
        if (_issues.CurrentRow?.Tag is string id) Core.Instance.RevealGlyph(Document, id);
    }

    private void SaveModule()
    {
        if (!_save.Enabled) return;
        using var dialog = new SaveFileDialog { Filter = "Python (*.py)|*.py", DefaultExt = "py", FileName = "models.py" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { File.WriteAllText(dialog.FileName, _code.Text); }
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
            Core.Instance.DocumentClosed -= DocumentClosed;
            _codeFont.Dispose();
        }
        base.Dispose(disposing);
    }
}

using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Tools.EntityRelationship;

internal sealed class EntityDiagramForm : Form
{
    private readonly string _language;
    private readonly string _title;
    private readonly Font _codeFont = new("Consolas", 10);
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly TextBox _sql = new()
    {
        ReadOnly = true, Multiline = true, WordWrap = false, Dock = DockStyle.Fill,
        ScrollBars = ScrollBars.Both, BackColor = SystemColors.Window, AccessibleName = "SQL"
    };
    private readonly DataGridView _errors = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, RowHeadersVisible = false,
        AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
    };
    private readonly SandboxPanel _sandbox;
    private bool _libraryExample;
    private string? _schema;
    public Document Document { get; }

    public EntityDiagramForm(Document document, string language, string title)
    {
        Document = document;
        _language = language;
        _title = title.TrimEnd('…');
        Size = new Size(700, 560);
        MinimumSize = new Size(460, 380);
        ShowInTaskbar = false;

        _sql.Font = _codeFont;
        var save = new Button { AutoSize = true, Text = T("Save .sql…", "Сохранить .sql…", "Зберегти .sql…") };
        save.Click += (_, _) => SaveSql();
        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
        bar.Controls.Add(save);
        var preview = new TabPage("SQL");
        preview.Controls.Add(_sql);
        preview.Controls.Add(bar);

        _sandbox = new SandboxPanel(language) { Dock = DockStyle.Fill };
        var test = new TabPage(T("Test database", "Проверка данных", "Перевірка даних"));
        test.Controls.Add(_sandbox);
        _tabs.TabPages.Add(preview);
        _tabs.TabPages.Add(test);
        _tabs.SelectedIndexChanged += (_, _) => UpdateSandbox();

        _errors.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _errors.Columns.Add("element", T("Element", "Элемент", "Елемент"));
        _errors.Columns.Add("message", T("What to fix", "Что исправить", "Що виправити"));
        _errors.Columns[0].FillWeight = 30;
        _errors.Columns[1].FillWeight = 70;
        _errors.CellClick += (_, e) => { if (e.RowIndex >= 0) RevealIssue(); };
        _errors.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            RevealIssue();
        };
        Controls.Add(_errors);
        Controls.Add(_tabs);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Document.Changed += DocumentChanged;
        Core.Instance.DocumentClosed += DocumentClosed;
        RefreshDiagram();
    }

    private void DocumentChanged(object? sender, EventArgs e) => RefreshDiagram();
    private void DocumentClosed(Document document) { if (document == Document) Close(); }
    private string T(string en, string ru, string uk) => _language switch { "Russian" => ru, "Ukrainian" => uk, _ => en };

    private void RefreshDiagram()
    {
        Text = $"{_title} — {Document.DisplayName}";
        var result = EntityDiagramReader.Analyze(Document, out _libraryExample);
        _schema = result.Sql;
        _tabs.Visible = result.IsValid;
        _errors.Visible = !result.IsValid;
        string preview = _schema?.ReplaceLineEndings(Environment.NewLine) ?? "";
        if (_sql.Text != preview) _sql.Text = preview;
        if (result.IsValid)
        {
            UpdateSandbox();
            return;
        }

        _sandbox.SetSchema(null);
        _errors.Rows.Clear();
        var glyphs = Document.DocumentSheet.Glyphs.ToLookup(glyph => glyph.Id);
        foreach (var error in result.Errors)
        {
            var glyph = error.Id == null ? null : glyphs[error.Id].FirstOrDefault();
            string name = glyph switch
            {
                ConnectorGlyph { Source: { } from, Target: { } to } => $"{from.Caption} → {to.Caption}",
                null => T("Diagram", "Схема", "Схема"),
                _ => string.IsNullOrWhiteSpace(glyph.Caption) ? Core.Instance.CurrentTranslator[glyph.Name] : glyph.Caption
            };
            var index = _errors.Rows.Add(name, error.GetMessage(_language));
            _errors.Rows[index].Tag = error.Id;
        }
    }

    private void UpdateSandbox()
    {
        if (_tabs.SelectedIndex == 1 && _schema != null) _sandbox.SetSchema(_schema, _libraryExample);
    }

    private void RevealIssue()
    {
        if (_errors.CurrentRow?.Tag is string id) Core.Instance.RevealGlyph(Document, id);
    }

    private void SaveSql()
    {
        if (_schema == null) return;
        using var dialog = new SaveFileDialog { Filter = "SQLite (*.sql)|*.sql", DefaultExt = "sql",
            FileName = _libraryExample ? "library.sql" : "schema.sql" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { File.WriteAllText(dialog.FileName, _schema); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            Document.Changed -= DocumentChanged;
            Core.Instance.DocumentClosed -= DocumentClosed;
            _codeFont.Dispose();
        }
        base.Dispose(disposing);
    }
}

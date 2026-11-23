using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Glyphs.States;

namespace Makarov.FlowchartBuilder.Tools.StateMachine;

internal sealed class MachineForm : Form
{
    private readonly string _language;
    private readonly string _title;
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly Label _current = new() { AutoSize = true, UseMnemonic = false, Margin = new Padding(4, 4, 4, 10) };
    private readonly FlowLayoutPanel _events = new() { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true };
    private readonly ListBox _history = new() { Dock = DockStyle.Fill, HorizontalScrollbar = true, IntegralHeight = false };
    private readonly Button _back = new() { AutoSize = true, Enabled = false };
    private readonly DataGridView _issues = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, RowHeadersVisible = false,
        AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
    };
    private readonly TextBox _code = new()
    {
        Multiline = true, ReadOnly = true, WordWrap = false, ScrollBars = ScrollBars.Both,
        Dock = DockStyle.Fill, BackColor = SystemColors.Window
    };
    private readonly Font _codeFont = new("Consolas", 10);
    private MachineRun? _run;
    private StateGlyph? _highlighted;

    public Document Document { get; }

    public MachineForm(Document document, string language, string title)
    {
        Document = document;
        _language = language;
        _title = title.TrimEnd('…');
        Size = new Size(520, 425);
        MinimumSize = new Size(340, 300);
        ShowInTaskbar = false;
        _tabs.TabPages.Add(CreateRunTab());
        _tabs.TabPages.Add(CreatePythonTab());

        _issues.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        AddIssueColumn(T("Element", "Элемент", "Елемент"), 30);
        AddIssueColumn(T("What to fix", "Что исправить", "Що виправити"), 70);
        _issues.CellClick += (_, e) => { if (e.RowIndex >= 0) RevealIssue(); };
        _issues.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            RevealIssue();
        };
        Controls.Add(_issues);
        Controls.Add(_tabs);
    }

    private string T(string en, string ru, string uk) => _language switch { "Russian" => ru, "Ukrainian" => uk, _ => en };

    private TabPage CreateRunTab()
    {
        var page = new TabPage(T("Transitions", "Переходы", "Переходи"));
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(8) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _events.AccessibleName = T("Available events", "Доступные события", "Доступні події");
        _history.AccessibleName = T("Transition history", "История переходов", "Історія переходів");
        _back.Text = T("Step back", "Шаг назад", "Крок назад");
        _back.Click += (_, _) =>
        {
            if (_run?.StepBack() != true) return;
            _history.Items.RemoveAt(_history.Items.Count - 1);
            ShowCurrent(reveal: true);
        };
        var reset = new Button { AutoSize = true, Text = T("Restart", "Начать заново", "Почати спочатку") };
        reset.Click += (_, _) =>
        {
            if (_run == null) return;
            _run.Reset();
            _history.Items.Clear();
            ShowCurrent(reveal: true);
        };
        var locate = new Button { AutoSize = true, Text = T("Show on canvas", "Показать на холсте", "Показати на полотні") };
        locate.Click += (_, _) => { if (_run != null) Core.Instance.RevealGlyph(Document, _run.Current.Id); };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        actions.Controls.AddRange([_back, reset, locate]);
        layout.Controls.Add(_current, 0, 0);
        layout.Controls.Add(new Label { AutoSize = true, Text = _events.AccessibleName }, 0, 1);
        layout.Controls.Add(_events, 0, 2);
        layout.Controls.Add(new Label { AutoSize = true, Text = _history.AccessibleName }, 0, 3);
        layout.Controls.Add(_history, 0, 4);
        layout.Controls.Add(actions, 0, 5);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreatePythonTab()
    {
        var page = new TabPage("Python");
        _code.Font = _codeFont;
        var save = new Button { AutoSize = true, Text = T("Save .py…", "Сохранить .py…", "Зберегти .py…") };
        save.Click += (_, _) => SaveModule();
        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
        bar.Controls.Add(save);
        page.Controls.Add(_code);
        page.Controls.Add(bar);
        return page;
    }

    private void AddIssueColumn(string title, int weight) => _issues.Columns.Add(new DataGridViewTextBoxColumn
    {
        HeaderText = title, FillWeight = weight, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        SortMode = DataGridViewColumnSortMode.NotSortable
    });

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (Owner is Form editor)
        {
            var work = Screen.FromControl(editor).WorkingArea;
            var canvas = Core.Instance.MainWindow.SheetView.RectangleToScreen(Core.Instance.MainWindow.SheetView.ClientRectangle);
            Location = new Point(Math.Max(work.Left, Math.Min(canvas.Right + 8, work.Right - Width)),
                                 Math.Max(work.Top, Math.Min(canvas.Top, work.Bottom - Height)));
        }
        Document.Changed += DocumentChanged;
        Core.Instance.DocumentClosed += DocumentClosed;
        RefreshDiagram();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (_run != null) Core.Instance.RevealGlyph(Document, _run.Current.Id);
    }

    private void DocumentChanged(object? sender, EventArgs e) => RefreshDiagram();

    private void DocumentClosed(Document document)
    {
        if (document == Document) Close();
    }

    private void RefreshDiagram()
    {
        Text = $"{_title} — {Document.DisplayName}";
        var analysis = StateDiagramReader.Analyze(Document);
        _issues.Rows.Clear();
        bool valid = analysis.Definition != null;
        _tabs.Visible = valid;
        _issues.Visible = !valid;
        if (analysis.Definition is not { } definition)
        {
            _run = null;
            SetHighlight(null);
            _code.Clear();
            var glyphs = Document.DocumentSheet.Glyphs.ToLookup(glyph => glyph.Id);
            foreach (var issue in analysis.Issues)
            {
                var glyph = issue.GlyphId == null ? null : glyphs[issue.GlyphId].FirstOrDefault();
                string label = glyph switch
                {
                    ConnectorGlyph { Source: { } from, Target: { } to } => $"{from.Caption} → {to.Caption}",
                    null => T("Diagram", "Схема", "Схема"),
                    _ => string.IsNullOrWhiteSpace(glyph.Caption) ? Core.Instance.CurrentTranslator[glyph.Name] : glyph.Caption
                };
                int row = _issues.Rows.Add(label, issue.Message(_language));
                _issues.Rows[row].Tag = issue.GlyphId;
            }
            return;
        }

        if (_run == null) _run = new MachineRun(definition);
        else _run.Update(definition);
        var module = definition.GeneratePython();
        if (_code.Text != module) _code.Text = module;
        _history.BeginUpdate();
        try
        {
            _history.Items.Clear();
            foreach (var step in _run.History) AppendHistory(step);
        }
        finally { _history.EndUpdate(); }
        ShowCurrent(reveal: false);
    }

    private void ShowCurrent(bool reveal)
    {
        if (_run == null) return;
        var current = _run.Current;
        _back.Enabled = _run.History.Count > 0;
        _current.Text = T("Current state", "Текущее состояние", "Поточний стан") + " — " + current.Name;
        var available = _run.Definition.Available(current.Id);
        _events.SuspendLayout();
        try
        {
            while (_events.Controls.Count > 0) _events.Controls[0].Dispose();
            foreach (var transition in available)
            {
                var button = new Button { Text = transition.Event, UseMnemonic = false, AutoSize = true };
                button.Click += (_, _) => Step(transition.Event);
                _events.Controls.Add(button);
            }
            if (available.Count == 0)
            {
                string message = current.IsFinal ? T("Final state", "Конечное состояние", "Кінцевий стан")
                    : T("No outgoing transitions", "Нет исходящих переходов", "Немає вихідних переходів");
                _events.Controls.Add(new Label { Text = message, AutoSize = true });
            }
        }
        finally { _events.ResumeLayout(); }

        SetHighlight(Document.DocumentSheet.Glyphs.OfType<StateGlyph>().First(glyph => glyph.Id == current.Id));
        if (reveal) Core.Instance.RevealGlyph(Document, current.Id);
    }

    private void SetHighlight(StateGlyph? current)
    {
        if (ReferenceEquals(_highlighted, current)) return;
        if (_highlighted != null) _highlighted.IsCurrent = false;
        _highlighted = current;
        if (current != null) current.IsCurrent = true;
        Core.Instance.Redraw();
    }

    private void Step(string eventName)
    {
        if (_run == null || !_run.Step(eventName)) return;
        AppendHistory(_run.History[^1]);
        ShowCurrent(reveal: true);
    }

    private void AppendHistory(MachineTransition step)
    {
        var definition = _run!.Definition;
        _history.Items.Add($"{definition.State(step.SourceId).Name}  — {step.Event} →  {definition.State(step.TargetId).Name}");
        _history.TopIndex = _history.Items.Count - 1;
    }

    private void RevealIssue()
    {
        if (_issues.CurrentRow?.Tag is string id) Core.Instance.RevealGlyph(Document, id);
    }

    private void SaveModule()
    {
        if (_run == null) return;
        using var dialog = new SaveFileDialog { Filter = "Python (*.py)|*.py", DefaultExt = "py", FileName = "state_machine.py" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { File.WriteAllText(dialog.FileName, _code.Text); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            Document.Changed -= DocumentChanged;
            Core.Instance.DocumentClosed -= DocumentClosed;
            SetHighlight(null);
            _codeFont.Dispose();
        }
        base.Dispose(disposing);
    }
}

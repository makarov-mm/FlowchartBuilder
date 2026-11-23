using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Tools.PythonExport;

public sealed class FlowchartValidationTool : IDocumentTool
{
    private static readonly Dictionary<Document, Form> Windows = [];

    public string ExampleFileName => "Countdown-exercise.mfb";

    public string ExampleInstructions(string language) => language switch
    {
        "Russian" => "Откройте «Плагины → Проверить схему». Найдите стрелку от условия к блоку конца и исправьте её подпись. Затем откройте «Блок-схема → Python». Почему программа выведет 3, 2, 1 и остановится до нуля?",
        "Ukrainian" => "Відкрийте «Плагіни → Перевірити схему». Знайдіть стрілку від умови до кінцевого блока та виправте її підпис. Потім відкрийте «Блок-схема → Python». Чому програма виведе 3, 2, 1 і зупиниться перед нулем?",
        _ => "Open Plugins → Check flowchart. Find the arrow from the decision to End and fix its label. Open Flowchart → Python. Why will the program print 3, 2, 1 and stop before zero?"
    };

    public string Title(string language) => language switch
    {
        "Russian" => "Проверить схему", "Ukrainian" => "Перевірити схему", _ => "Check flowchart"
    };

    public void Run(Document document, IWin32Window owner, string language)
    {
        if (Windows.TryGetValue(document, out var existing))
        {
            existing.Activate();
            return;
        }

        var form = new ValidationForm(document, language, Title(language));
        Windows.Add(document, form);
        form.Disposed += (_, _) => Windows.Remove(document);
        form.Show(owner);
    }

    private sealed class ValidationForm : Form
    {
        private readonly Document _document;
        private readonly string _language;
        private readonly string _title;
        private readonly Label _summary = new() { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6) };
        private readonly DataGridView _list = new()
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
        };
        private readonly Button _show = new() { AutoSize = true };
        private bool _updating;

        public ValidationForm(Document document, string language, string title)
        {
            _document = document;
            _language = language;
            _title = title;
            Size = new Size(550, 360);
            MinimumSize = new Size(380, 200);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;

            _list.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            _list.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = Translate("Element", "Элемент", "Елемент"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 30,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            _list.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = Translate("What to fix", "Что исправить", "Що виправити"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 70,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            _show.Text = Translate("Show on canvas", "Показать на холсте", "Показати на полотні");
            _show.Click += (_, _) => Reveal();
            _list.SelectionChanged += (_, _) => { if (!_updating) Reveal(); };
            _list.CellClick += (_, _) => Reveal();
            var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
            actions.Controls.Add(_show);
            Controls.Add(_list);
            Controls.Add(_summary);
            Controls.Add(actions);
            _document.Changed += DocumentChanged;
            Core.Instance.DocumentClosed += DocumentClosed;
            RefreshDiagnostics();
        }

        private string Translate(string english, string russian, string ukrainian) => _language switch
        {
            "Russian" => russian, "Ukrainian" => ukrainian, _ => english
        };

        private void DocumentChanged(object? sender, EventArgs args) => RefreshDiagnostics();

        private void DocumentClosed(Document document)
        {
            if (ReferenceEquals(document, _document)) Close();
        }

        private void RefreshDiagnostics()
        {
            var diagnostics = FlowchartValidator.Analyze(_document).Diagnostics;
            var elements = _document.DocumentSheet.Glyphs.ToLookup(glyph => glyph.Id);
            string? previous = (_list.CurrentRow?.Tag as FlowchartDiagnostic)?.GlyphId;
            _updating = true;
            try
            {
                Text = $"{_title} — {_document.DisplayName}";
                _list.Rows.Clear();
                DataGridViewRow? next = null;
                foreach (var diagnostic in diagnostics)
                {
                    var glyph = diagnostic.GlyphId is null ? null : elements[diagnostic.GlyphId].FirstOrDefault();
                    int index = _list.Rows.Add(ElementLabel(glyph), diagnostic.Message(_language));
                    var row = _list.Rows[index];
                    row.Tag = diagnostic;
                    if (previous is not null && previous == diagnostic.GlyphId) next ??= row;
                }
                if (next is not null) _list.CurrentCell = next.Cells[0];
                _summary.Text = diagnostics.Count == 0
                    ? Translate("No flowchart issues found.", "Ошибок в структуре схемы не найдено.", "Помилок у структурі схеми не знайдено.")
                    : Translate($"Issues: {diagnostics.Count}", $"Замечаний: {diagnostics.Count}", $"Зауважень: {diagnostics.Count}");
                _show.Enabled = _list.CurrentRow?.Tag is FlowchartDiagnostic { GlyphId: not null };
            }
            finally { _updating = false; }
        }

        private string ElementLabel(AbstractGlyph? glyph)
        {
            if (glyph is null) return Translate("Document", "Документ", "Документ");
            if (glyph is ConnectorGlyph { Source: { } source, Target: { } target })
                return $"{BlockLabel(source)} → {BlockLabel(target)}";
            return BlockLabel(glyph);
        }

        private static string BlockLabel(AbstractGlyph glyph) =>
            string.IsNullOrWhiteSpace(glyph.Caption) ? Core.Instance.CurrentTranslator[glyph.Name] : glyph.Caption;

        private void Reveal()
        {
            string? id = (_list.CurrentRow?.Tag as FlowchartDiagnostic)?.GlyphId;
            _show.Enabled = id is not null;
            if (id is not null) Core.Instance.RevealGlyph(_document, id);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _document.Changed -= DocumentChanged;
                Core.Instance.DocumentClosed -= DocumentClosed;
            }
            base.Dispose(disposing);
        }
    }
}

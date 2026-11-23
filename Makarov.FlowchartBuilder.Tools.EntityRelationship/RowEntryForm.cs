namespace Makarov.FlowchartBuilder.Tools.EntityRelationship;

internal sealed class RowEntryForm : Form
{
    private enum ValueMode { Value, Automatic, Null }
    private sealed record ModeOption(ValueMode Mode, string Label)
    {
        public override string ToString() => Label;
    }

    private sealed record ReferenceOption(EntryReference Reference)
    {
        public override string ToString() => string.IsNullOrEmpty(Reference.Label)
            ? Reference.Key : Reference.Key + " — " + Reference.Label;
    }

    private sealed record Field(EntryColumn Column, Control Input, ComboBox? Mode)
    {
        public ValueMode Selected => Mode?.SelectedItem is ModeOption option ? option.Mode : ValueMode.Value;

        public string Text => Input is ComboBox { SelectedItem: ReferenceOption current } && Input.Text == current.ToString()
            ? current.Reference.Key : Input.Text;
    }

    private readonly string _language;
    private readonly Action<IReadOnlyDictionary<string, EntryValue>> _insert;
    private readonly List<Field> _fields = [];
    private readonly ToolTip _tips = new();
    private readonly Label _status = new()
    {
        Name = "entryStatus", AutoSize = true, AutoEllipsis = true, UseMnemonic = false,
        Visible = false, Margin = new Padding(3, 6, 3, 6)
    };

    public RowEntryForm(string language, string table, IReadOnlyList<EntryColumn> columns,
        Func<EntryColumn, IReadOnlyList<EntryReference>> getReferenceValues, Action<IReadOnlyDictionary<string, EntryValue>> insert)
    {
        _language = language;
        _insert = insert;
        Text = T("Add row", "Добавить строку", "Додати рядок") + " — " + table;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = false;
        MinimumSize = new Size(480, 280);
        ClientSize = new Size(650, 135 + Math.Clamp(columns.Count, 3, 8) * 54);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), RowCount = 3, ColumnCount = 1 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Margin = Padding.Empty };
        var grid = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 3, RowCount = 1 };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67));
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.Controls.Add(Header(T("Column", "Поле", "Поле")), 0, 0);
        grid.Controls.Add(Header(T("Mode", "Режим", "Режим")), 1, 0);
        grid.Controls.Add(Header(T("Value", "Значение", "Значення")), 2, 0);
        foreach (var column in columns) AddField(grid, column, getReferenceValues);
        scroll.Controls.Add(grid);

        _status.AccessibleName = T("Entry error", "Ошибка ввода", "Помилка введення");
        layout.SizeChanged += (_, _) =>
            _status.MaximumSize = new Size(Math.Max(1, layout.ClientSize.Width - layout.Padding.Horizontal - 8), Font.Height * 4);
        var add = new Button { AutoSize = true, Text = T("Add", "Добавить", "Додати") };
        add.Click += (_, _) => Submit();
        var cancel = new Button { AutoSize = true, Text = T("Cancel", "Отмена", "Скасувати"), DialogResult = DialogResult.Cancel };
        var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        actions.Controls.AddRange([cancel, add]);
        layout.Controls.Add(scroll, 0, 0);
        layout.Controls.Add(_status, 0, 1);
        layout.Controls.Add(actions, 0, 2);
        Controls.Add(layout);
        AcceptButton = add;
        CancelButton = cancel;
        Shown += (_, _) => _fields.FirstOrDefault(f => f.Input.Enabled)?.Input.Focus();
    }

    private static Label Header(string text) => new() { AutoSize = true, UseMnemonic = false, Text = text };

    private void AddField(TableLayoutPanel grid, EntryColumn column, Func<EntryColumn, IReadOnlyList<EntryReference>> getReferenceValues)
    {
        int row = grid.RowCount++;
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        string details = column.Type;
        if (column.ParentTable != null) details += $" → {column.ParentTable}.{column.ParentColumn}";
        var label = new Label
        {
            Text = column.Name + Environment.NewLine + details, UseMnemonic = false,
            AutoEllipsis = true, Dock = DockStyle.Fill, MinimumSize = new Size(0, Font.Height * 2 + 8)
        };
        _tips.SetToolTip(label, label.Text);
        grid.Controls.Add(label, 0, row);

        Control input;
        if (column.ParentTable != null)
        {
            var keys = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
            try
            {
                foreach (var key in getReferenceValues(column)) keys.Items.Add(new ReferenceOption(key));
            }
            catch (Exception error) when (error is InvalidOperationException or ArgumentException)
            {
                string hint = T("Enter the key manually.", "Введите ключ вручную.", "Введіть ключ вручну.");
                ShowError(column.Name + " — " + hint + " " + error.Message);
            }
            input = keys;
        }
        else if (column.Type == "TEXT")
        {
            input = new TextBox { Multiline = true, AcceptsReturn = true, ScrollBars = ScrollBars.Vertical, Height = Font.Height * 2 + 8 };
        }
        else
        {
            input = new TextBox { PlaceholderText = column.Type == "BLOB" ? T("Hex bytes", "Байты в hex", "Байти у hex") : "" };
        }
        if (input is TextBox textBox) textBox.MaxLength = 0;
        input.TabIndex = row * 2 + 1;
        input.AccessibleName = column.Name;
        input.AccessibleDescription = details;
        input.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        grid.Controls.Add(input, 2, row);

        ComboBox? mode = null;
        if (!column.Required || column.AutoId)
        {
            mode = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left, TabIndex = row * 2,
                AccessibleName = T("Mode", "Режим", "Режим") + " — " + column.Name
            };
            var automatic = T("Automatic", "Автоматически", "Автоматично");
            mode.Items.Add(column.AutoId ? new ModeOption(ValueMode.Automatic, automatic) : new ModeOption(ValueMode.Null, "NULL"));
            mode.Items.Add(new ModeOption(ValueMode.Value, T("Value", "Значение", "Значення")));
            mode.Width = Math.Max(mode.Width, TextRenderer.MeasureText(automatic, mode.Font).Width + SystemInformation.VerticalScrollBarWidth + 12);
            mode.SelectedIndexChanged += (_, _) => input.Enabled = mode.SelectedItem is ModeOption { Mode: ValueMode.Value };
            mode.SelectedIndex = 0;
            grid.Controls.Add(mode, 1, row);
        }
        else
        {
            var required = Header(T("Required", "Обязательно", "Обов’язково"));
            required.Anchor = AnchorStyles.Left;
            grid.Controls.Add(required, 1, row);
        }
        _fields.Add(new Field(column, input, mode));
    }

    private void Submit()
    {
        Dictionary<string, EntryValue> values = new(StringComparer.Ordinal);
        foreach (var field in _fields)
        {
            if (field.Selected == ValueMode.Automatic) continue;
            values.Add(field.Column.Name, new EntryValue(field.Selected == ValueMode.Null ? null : field.Text));
        }
        try
        {
            _insert(values);
            DialogResult = DialogResult.OK;
        }
        catch (EntryInputException error)
        {
            string hint = error.Column.Type switch
            {
                "INTEGER" => T("Enter a 64-bit integer.", "Введите целое 64-битное число.", "Введіть ціле 64-бітне число."),
                "REAL" or "NUMERIC" => T("Enter a number using a decimal point.", "Введите число с точкой в качестве десятичного разделителя.", "Введіть число з крапкою як десятковим роздільником."),
                "BLOB" => T("Use two hexadecimal digits per byte.", "Введите по две шестнадцатеричные цифры на байт.", "Введіть по дві шістнадцяткові цифри на байт."),
                _ => error.Message
            };
            ShowError(error.Column.Name + " — " + hint);
            _fields.FirstOrDefault(f => f.Column.Name == error.Column.Name)?.Input.Focus();
        }
        catch (Exception error) when (error is InvalidOperationException or ArgumentException or OverflowException or FormatException)
        {
            string message = error.Message;
            string? explanation = message.StartsWith("FOREIGN KEY constraint failed", StringComparison.OrdinalIgnoreCase)
                ? T("Choose an existing key in the related table.", "Выберите существующий ключ связанной таблицы.", "Виберіть наявний ключ пов’язаної таблиці.")
                : message.StartsWith("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase)
                    ? T("This value must be unique.", "Значение должно быть уникальным.", "Значення має бути унікальним.")
                    : null;
            ShowError(explanation == null ? message : explanation + " " + message);
        }
    }

    private void ShowError(string message)
    {
        _status.Text = message;
        _tips.SetToolTip(_status, message);
        _status.Visible = true;
    }

    private string T(string en, string ru, string uk) => _language switch { "Russian" => ru, "Ukrainian" => uk, _ => en };

    protected override void Dispose(bool disposing)
    {
        if (disposing) _tips.Dispose();
        base.Dispose(disposing);
    }
}

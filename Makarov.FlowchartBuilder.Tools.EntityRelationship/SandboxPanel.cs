namespace Makarov.FlowchartBuilder.Tools.EntityRelationship;

internal sealed class SandboxPanel : UserControl
{
    private sealed record Preset(string Name, string Sql)
    {
        public override string ToString() => Name;
    }

    private readonly string _language;
    private readonly Font _codeFont = new("Consolas", 10);
    private readonly TextBox _query = new()
    {
        Multiline = true, WordWrap = false, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Both, AcceptsReturn = true
    };
    private readonly ComboBox _presets = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly Button _run = new() { AutoSize = true };
    private readonly Button _reset = new() { AutoSize = true };
    private readonly ComboBox _entryTable = new()
    {
        Name = "entryTable", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly Button _addRow = new() { Name = "addRow", AutoSize = true };
    private readonly Label _status = new() { AutoSize = true, Dock = DockStyle.Fill, UseMnemonic = false };
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        AllowUserToOrderColumns = true, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
    };
    private readonly SandboxSession _session = new();
    private string _lastStatus = "";

    public SandboxPanel(string language)
    {
        _language = language;
        Dock = DockStyle.Fill;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 7, ColumnCount = 1, Padding = new Padding(6) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 46));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 54));
        _query.Font = _codeFont;
        _query.AccessibleName = T("SQL query", "SQL-запрос", "SQL-запит");
        _presets.AccessibleName = T("Query templates", "Шаблоны запросов", "Шаблони запитів");
        _presets.SelectedIndexChanged += (_, _) =>
        {
            if (_presets.SelectedItem is Preset preset)
            {
                string text = preset.Sql.ReplaceLineEndings(Environment.NewLine);
                if (_query.Text != text) _query.Text = text;
            }
        };
        _query.TextChanged += (_, _) =>
        {
            if (_presets.SelectedItem is Preset preset && _query.Text != preset.Sql.ReplaceLineEndings(Environment.NewLine))
                _presets.SelectedIndex = -1;
        };
        _grid.AccessibleName = T("Query result", "Результат запроса", "Результат запиту");
        _grid.DefaultCellStyle.NullValue = "NULL";
        _run.Text = T("Run SQL", "Выполнить SQL", "Виконати SQL");
        _run.Click += (_, _) => ExecuteQuery();
        _query.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter || !e.Control) return;
            e.SuppressKeyPress = true;
            ExecuteQuery();
        };
        _reset.Click += (_, _) => ResetDatabase();
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        actions.Controls.AddRange([_run, _reset]);
        _entryTable.AccessibleName = T("Table", "Таблица", "Таблиця");
        _addRow.Text = T("Add row…", "Добавить строку…", "Додати рядок…");
        _addRow.Click += (_, _) => AddRow();
        var entry = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 3, RowCount = 1 };
        entry.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        entry.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        entry.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        entry.Controls.Add(new Label { Text = _entryTable.AccessibleName, AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        entry.Controls.Add(_entryTable, 1, 0);
        entry.Controls.Add(_addRow, 2, 0);
        layout.Controls.Add(new Label { AutoSize = true, Text = _presets.AccessibleName }, 0, 0);
        layout.Controls.Add(_presets, 0, 1);
        layout.Controls.Add(_query, 0, 2);
        layout.Controls.Add(actions, 0, 3);
        layout.Controls.Add(entry, 0, 4);
        layout.Controls.Add(_status, 0, 5);
        layout.Controls.Add(_grid, 0, 6);
        Controls.Add(layout);
        UpdateActions();
    }

    public void SetSchema(string? sql, bool libraryExample = false)
    {
        if (!_session.SetSchema(sql, libraryExample)) return;
        if (!_session.HasDatabase && _session.HasSchema)
        {
            ResetDatabase();
            return;
        }
        UpdateActions();
        _status.Text = _session.CanRun ? _lastStatus : _session.HasSchema
            ? T("Schema changed. Rebuilding clears the test data. Undo the schema change to continue with the current data.",
                "Схема изменилась. Пересоздание очистит тестовые данные. Отмените правку схемы, чтобы продолжить с текущими данными.",
                "Схема змінилася. Перестворення очистить тестові дані. Скасуйте зміну схеми, щоб продовжити з поточними даними.")
            : T("Fix the schema to continue.", "Исправьте схему, чтобы продолжить.", "Виправте схему, щоб продовжити.");
    }

    private void UpdateActions()
    {
        _run.Enabled = _presets.Enabled = _session.CanRun;
        _entryTable.Enabled = _addRow.Enabled = _session.CanRun && _entryTable.Items.Count > 0;
        _reset.Enabled = _session.HasSchema;
        _reset.Text = _session.HasDatabase && !_session.CanRun
            ? T("Rebuild database", "Пересоздать базу", "Перестворити базу")
            : T("Reset data", "Сбросить данные", "Скинути дані");
    }

    private void PopulatePresets(bool useDefault)
    {
        string draft = _query.Text;
        _presets.Items.Clear();
        if (_session.HasLibraryData)
        {
            Add(T("Lend copy 1 to Anna", "Выдать экземпляр 1 Анне", "Видати примірник 1 Анні"), SandboxExamples.Issue(1, 1));
            Add(T("Lend copy 1 to Oleg", "Выдать экземпляр 1 Олегу", "Видати примірник 1 Олегові"), SandboxExamples.Issue(1, 2));
            Add(T("Lend copy 2 to Oleg", "Выдать экземпляр 2 Олегу", "Видати примірник 2 Олегові"), SandboxExamples.Issue(2, 2));
            Add(T("Return copy 1", "Вернуть экземпляр 1", "Повернути примірник 1"), SandboxExamples.ReturnFirst);
            Add(T("Show loans", "Посмотреть выдачи", "Переглянути видачі"), SandboxExamples.Loans);
        }
        Add(T("List tables", "Список таблиц", "Список таблиць"), SandboxExamples.Tables);
        string? previousTable = _entryTable.SelectedItem as string;
        _entryTable.Items.Clear();
        foreach (var table in _session.TableNames)
        {
            Add(T($"Rows in {table}", $"Строки таблицы {table}", $"Рядки таблиці {table}"), SandboxExamples.ReadTable(table));
            _entryTable.Items.Add(table);
        }
        if (_entryTable.Items.Count > 0)
            _entryTable.SelectedIndex = previousTable == null ? 0 : Math.Max(0, _entryTable.Items.IndexOf(previousTable));
        if (useDefault && draft.Length == 0)
            _presets.SelectedIndex = 0;
        else
            _presets.SelectedItem = _presets.Items.Cast<Preset>().FirstOrDefault(p => p.Sql.ReplaceLineEndings(Environment.NewLine) == draft);
    }

    private void ResetDatabase()
    {
        if (!_session.HasSchema) return;
        bool first = !_session.HasDatabase;
        try
        {
            string? seedError = _session.Rebuild();
            PopulatePresets(first);
            _grid.Columns.Clear();
            _lastStatus = seedError != null
                ? T("Empty database ready. Sample data did not fit the schema",
                    "Пустая база готова. Пример данных не подходит к схеме",
                    "Порожня база готова. Зразок даних не підходить до схеми") + " — " + seedError
                : _session.HasLibraryData
                    ? T("Ready · 1 book, 2 copies, 2 readers", "База готова · 1 книга, 2 экземпляра, 2 читателя", "База готова · 1 книга, 2 примірники, 2 читачі")
                    : T("Empty database ready", "Пустая база готова", "Порожня база готова");
            _status.Text = _lastStatus;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or DllNotFoundException or EntryPointNotFoundException or BadImageFormatException or TypeInitializationException)
        {
            _status.Text = T("SQLite error", "Ошибка SQLite", "Помилка SQLite") + " — " + ex.GetBaseException().Message;
        }
        UpdateActions();
    }

    private void Add(string label, string sql) => _presets.Items.Add(new Preset(label, sql));

    private void AddRow()
    {
        if (!_session.CanRun || _entryTable.SelectedItem is not string table) return;
        var database = _session.ActiveDatabase;
        long rowId = 0;
        string? idColumn;
        try
        {
            var columns = database.GetEntryColumns(table);
            idColumn = columns.FirstOrDefault(c => c.AutoId)?.Name;
            using var dialog = new RowEntryForm(_language, table, columns, database.GetReferenceValues, values =>
            {
                if (!_session.CanRun || _session.ActiveDatabase != database)
                    throw new InvalidOperationException(T("The schema changed. Reopen the form.", "Схема изменилась. Откройте форму заново.", "Схема змінилася. Відкрийте форму знову."));
                try { rowId = database.InsertRow(table, values); }
                catch (InvalidOperationException ex) when (IsDuplicateLoan(ex))
                {
                    throw new InvalidOperationException(OccupiedCopyMessage() + " — " + ex.Message, ex);
                }
            });
            if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            _status.Text = _lastStatus = T("SQLite error", "Ошибка SQLite", "Помилка SQLite") + " — " + ex.Message;
            return;
        }
        string message = T($"Row added to {table}.", $"Строка добавлена в {table}.", $"Рядок додано до {table}.");
        if (idColumn != null) message += $" {idColumn} = {rowId}.";
        try
        {
            var preview = database.PreviewInsertedRow(table, rowId);
            ShowResult(preview);
            if (preview.MoreRows)
                message += " " + T($"Showing {preview.Rows.Count} rows.", $"Показано строк: {preview.Rows.Count}.", $"Показано рядків: {preview.Rows.Count}.");
        }
        catch (InvalidOperationException ex)
        {
            _grid.Columns.Clear();
            message += " " + T("Could not refresh the table", "Не удалось обновить таблицу", "Не вдалося оновити таблицю") + " — " + ex.Message;
        }
        _status.Text = _lastStatus = message;
    }

    private bool IsDuplicateLoan(InvalidOperationException error) => _session.HasLibraryData &&
        error.Message.Equals("UNIQUE constraint failed: loans.copy_id", StringComparison.OrdinalIgnoreCase);

    private string OccupiedCopyMessage() => T("This copy is already on loan. Return it before lending it again.",
        "Экземпляр уже выдан. Сначала оформите возврат.", "Примірник уже видано. Спочатку оформіть повернення.");

    private void ShowResult(SandboxResult result)
    {
        _grid.Columns.Clear();
        for (int i = 0; i < result.Columns.Count; i++)
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "c" + i, HeaderText = result.Columns[i], SortMode = DataGridViewColumnSortMode.NotSortable });
        foreach (var row in result.Rows) _grid.Rows.Add(row.Select(cell => (object?)cell ?? DBNull.Value).ToArray());
    }

    private void ExecuteQuery()
    {
        if (!_session.CanRun || string.IsNullOrWhiteSpace(_query.Text)) return;
        try
        {
            var result = _session.Execute(_query.Text);
            ShowResult(result);
            string count = result.MoreRows ? result.Rows.Count + "+" : result.Rows.Count.ToString();
            _status.Text = T($"Rows changed: {result.Changes}   ·   Result rows: {count}",
                $"Строк изменено: {result.Changes}   ·   Строк в результате: {count}",
                $"Змінено рядків: {result.Changes}   ·   Рядків у результаті: {count}");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            _grid.Columns.Clear();
            string explanation = ex is InvalidOperationException dbError && IsDuplicateLoan(dbError)
                ? OccupiedCopyMessage()
                : T("Query rolled back", "Запрос отменён", "Запит скасовано");
            _status.Text = explanation + " — " + ex.Message;
        }
        _lastStatus = _status.Text;
    }

    private string T(string en, string ru, string uk) => _language switch { "Russian" => ru, "Ukrainian" => uk, _ => en };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _session.Dispose();
            _codeFont.Dispose();
        }
        base.Dispose(disposing);
    }
}

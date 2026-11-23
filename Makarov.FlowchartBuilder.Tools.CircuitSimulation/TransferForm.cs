using System.Numerics;

namespace Makarov.FlowchartBuilder.Tools.CircuitSimulation;

internal sealed class TransferForm : Form
{
    private readonly CircuitSimulator _model;
    private readonly string _language;
    private readonly double[] _frequencies = CircuitSimulator.LogFrequencies(1, 100_000, 40);
    private readonly ComboBox _input = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
    private readonly ComboBox _output = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
    private readonly NumericUpDown _cursor = new() { Minimum = 1, Maximum = 100_000, Value = 100, DecimalPlaces = 2, Width = 105 };
    private readonly TraceView _gain = new() { Dock = DockStyle.Fill, Unit = "dB", Cursor = Cursors.Cross };
    private readonly TraceView _phase = new() { Dock = DockStyle.Fill, Unit = "°", Cursor = Cursors.Cross };
    private readonly Label _reading = new() { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(8) };
    private readonly CancellationTokenSource _lifetime = new();
    private Complex[][]? _solutions;

    public TransferForm(CircuitSimulator model, string language)
    {
        _model = model;
        _language = language;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 570);
        MinimumSize = new Size(580, 440);

        var nodes = model.Network.Nodes;
        // Ground is a valid output. An input needs a non-zero reference signal.
        foreach (string name in nodes.Skip(1)) _input.Items.Add(name);
        foreach (string name in nodes) _output.Items.Add(name);
        var source = model.Network.Parts.First(p => p.Kind == SymbolKind.AcSource);
        int input = Math.Max(1, source.NodeA == 0 ? source.NodeB : source.NodeA);
        _input.SelectedIndex = input - 1;
        _output.SelectedIndex = Enumerable.Range(1, nodes.Count - 1).FirstOrDefault(n => n != input, input);

        var options = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4) };
        void AddOption(string label, Control control)
        {
            options.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(3, 6, 0, 0) });
            control.AccessibleName = label;
            options.Controls.Add(control);
        }
        AddOption(T("Input", "Вход", "Вхід"), _input);
        AddOption(T("Output", "Выход", "Вихід"), _output);
        AddOption(T("Cursor (Hz)", "Курсор (Гц)", "Курсор (Гц)"), _cursor);

        var plots = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        plots.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        plots.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        plots.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        void AddPlot(TraceView plot, string label, int row)
        {
            var frame = new GroupBox { Text = label, Dock = DockStyle.Fill, Padding = new Padding(4) };
            plot.FrequencyPicked += frequency => _cursor.Value =
                Math.Clamp(Math.Round((decimal)frequency, _cursor.DecimalPlaces), _cursor.Minimum, _cursor.Maximum);
            frame.Controls.Add(plot);
            plots.Controls.Add(frame, 0, row);
        }
        AddPlot(_gain, T("Voltage gain (dB)", "Усиление по напряжению (дБ)", "Підсилення за напругою (дБ)"), 0);
        AddPlot(_phase, T("Phase shift (°)", "Сдвиг фазы (°)", "Зсув фази (°)"), 1);
        Controls.Add(plots);
        Controls.Add(_reading);
        Controls.Add(options);

        _reading.Text = T("Calculating…", "Идёт расчёт…", "Триває розрахунок…");
        _input.SelectedIndexChanged += (_, _) => RefreshCurves();
        _output.SelectedIndexChanged += (_, _) => RefreshCurves();
        _cursor.ValueChanged += (_, _) => ReadCursor();
        Shown += async (_, _) => await LoadSweepAsync();
    }

    private string T(string en, string ru, string uk) => _language switch { "Russian" => ru, "Ukrainian" => uk, _ => en };

    private async Task LoadSweepAsync()
    {
        var token = _lifetime.Token;
        try
        {
            var solutions = await Task.Run(() => _frequencies.Select(frequency =>
            {
                token.ThrowIfCancellationRequested();
                return _model.SolveAc(frequency);
            }).ToArray(), token);
            if (token.IsCancellationRequested) return;
            _solutions = solutions;
            RefreshCurves();
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (InvalidOperationException error) { if (!token.IsCancellationRequested) _reading.Text = error.Message; }
    }

    private TransferResponse Measure(Complex[] solution) =>
        TransferResponse.Measure(solution, _input.SelectedIndex + 1, _output.SelectedIndex);

    private void RefreshCurves()
    {
        if (_solutions == null) return;
        var response = _solutions.Select(Measure).ToArray();
        _gain.ShowFrequencyCurve(_frequencies.Select((f, i) => (f, response[i].GainDb)));
        _phase.ShowFrequencyCurve(_frequencies.Select((f, i) => (f, response[i].PhaseDegrees)));
        ReadCursor();
    }

    private void ReadCursor()
    {
        if (_solutions == null) return;
        double frequency = (double)_cursor.Value;
        _gain.SetFrequencyCursor(frequency);
        _phase.SetFrequencyCursor(frequency);
        try
        {
            // Solve at the requested frequency, including points between sweep samples.
            var response = Measure(_model.SolveAc(frequency));
            string value;
            if (response.GainDb == null)
                value = T("Input voltage is zero", "Входное напряжение равно нулю", "Вхідна напруга дорівнює нулю");
            else if (response.PhaseDegrees == null)
                value = "−∞ dB   " + T("Phase undefined", "Фаза не определена", "Фазу не визначено");
            else
                value = $"{response.GainDb:0.00} dB     {response.PhaseDegrees:0.00}°";
            _reading.Text = $"{frequency:0.##} {T("Hz", "Гц", "Гц")}     {value}";
        }
        catch (InvalidOperationException error) { _reading.Text = error.Message; }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }
        base.Dispose(disposing);
    }
}

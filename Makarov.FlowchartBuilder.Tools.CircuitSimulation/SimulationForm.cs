using System.Diagnostics;

namespace Makarov.FlowchartBuilder.Tools.CircuitSimulation;

internal sealed class SimulationForm : Form
{
    private readonly CircuitSimulator _model;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 30 };
    private readonly TraceView _trace = new() { Dock = DockStyle.Fill };
    private readonly ListBox _probes = new() { Dock = DockStyle.Fill, IntegralHeight = false };
    private readonly NumericUpDown _step = new() { Minimum = 1, Maximum = 100000, Value = 100, Width = 82 };
    private readonly Label _reading = new() { AutoSize = true, Padding = new Padding(5) };
    private readonly Button _run = new() { AutoSize = true };
    private readonly Button _sweep = new() { AutoSize = true };
    private readonly string _language;
    private bool _fault;
    private readonly CancellationTokenSource _lifetime = new();

    public SimulationForm(CircuitSimulator model, string language)
    {
        _model = model;
        _language = language;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 480);
        MinimumSize = new Size(660, 360);
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4) };
        var reset = new Button { Text = T("Reset", "Сброс", "Скинути"), AutoSize = true };
        var dc = new Button { Text = T("DC operating point", "Расчёт DC", "Розрахунок DC"), AutoSize = true };
        _sweep.Text = T("AC sweep", "АЧХ", "АЧХ");
        var transfer = new Button
        {
            AutoSize = true, Enabled = model.HasAcSource && model.Network.Nodes.Count > 1,
            Text = T("Gain and phase…", "Усиление и фаза…", "Підсилення та фаза…")
        };
        transfer.Click += (_, _) =>
        {
            SetRunning(false);
            using var form = new TransferForm(_model, _language) { Text = transfer.Text.TrimEnd('…') };
            form.ShowDialog(this);
        };
        bar.Controls.AddRange([_run, reset, dc, new Label { AutoSize = true, Text = T("Step (µs)", "Шаг (мкс)", "Крок (мкс)"), Padding = new Padding(4,7,0,0) }, _step, _sweep, transfer, _reading]);
        var split = new SplitContainer { Dock = DockStyle.Fill, Width = 860, SplitterDistance = 220 };
        split.Panel1.Controls.Add(_probes);
        split.Panel2.Controls.Add(_trace);
        Controls.Add(split);
        Controls.Add(bar);
        foreach (string node in model.Network.Nodes) _probes.Items.Add("V  " + node);
        foreach (var part in model.Network.Parts) _probes.Items.Add("I  " + part.DisplayName);
        _probes.SelectedIndexChanged += (_, _) => { _trace.Frequency = false; _trace.ClearTrace(); RefreshReading(); };
        _probes.SelectedIndex = Math.Min(1, model.Network.Nodes.Count - 1);
        var switches = model.Network.Parts.Where(p => p.Kind == SymbolKind.Switch).ToList();
        if (switches.Count > 0)
        {
            var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
            foreach (var part in switches)
            {
                var check = new CheckBox { AutoSize = true, Text = part.DisplayName + " " + T("closed","замкнут","замкнено"), Checked = part.StateOn };
                check.CheckedChanged += async (_, _) => { part.StateOn = check.Checked; if (_trace.Frequency) await SweepAsync(); };
                panel.Controls.Add(check);
            }
            Controls.Add(panel);
        }
        if (model.Warnings.Count > 0)
        {
            var warnings = new TextBox { Dock = DockStyle.Bottom, ReadOnly = true, Multiline = true,
                Height = 48, ScrollBars = ScrollBars.Vertical, Text = string.Join(Environment.NewLine, model.Warnings) };
            Controls.Add(warnings);
        }
        _timer.Tick += (_, _) => Advance();
        _run.Click += (_, _) => { if (_trace.Frequency) { _trace.Frequency = false; _trace.ClearTrace(); } SetRunning(!_timer.Enabled); };
        reset.Click += (_, _) => { SetRunning(false); _model.Reset(); _fault = false; _run.Enabled = true; _trace.Frequency = false; _trace.ClearTrace(); RefreshReading(); };
        dc.Click += (_, _) =>
        {
            SetRunning(false);
            try
            {
                _model.SolveOperatingPoint();
                _fault = false;
                _run.Enabled = true;
                _trace.Frequency = false;
                _trace.ClearTrace();
                RefreshReading();
            }
            catch (InvalidOperationException e) { Fault(e); }
        };
        _sweep.Click += async (_, _) => await SweepAsync();
        SetRunning(false);
    }

    private string T(string en, string ru, string uk) => _language switch { "Russian" => ru, "Ukrainian" => uk, _ => en };
    private bool Voltage => _probes.SelectedIndex < _model.Network.Nodes.Count;
    private CircuitPart SelectedPart => _model.Network.Parts[_probes.SelectedIndex - _model.Network.Nodes.Count];
    private double Reading => Voltage ? _model.GetNodeVoltage(_probes.SelectedIndex) : _model.GetCurrent(SelectedPart) ?? 0;

    private void SetRunning(bool run)
    {
        _timer.Enabled = run;
        _run.Text = run ? T("Pause", "Пауза", "Пауза") : T("Run", "Пуск", "Пуск");
    }

    private void RefreshReading()
    {
        _trace.Unit = Voltage ? "V" : "A";
        _sweep.Enabled = !_fault && Voltage && _model.HasAcSource;
        _reading.Text = $"t = {Units.Format(_model.Time, "s")}   {Units.Format(Reading, _trace.Unit)}";
        if (!Voltage && SelectedPart.Kind == SymbolKind.Lamp) _reading.Text += $" ({_model.GetLampBrightness(SelectedPart):P0})";
        if (!Voltage && _model.IsFuseBlown(SelectedPart)) _reading.Text += T(" (blown)", " (перегорел)", " (перегорів)");
    }

    private void Advance()
    {
        try
        {
            long start = Stopwatch.GetTimestamp();
            for (int i = 0; i < 50; i++)
            {
                _model.Step((double)_step.Value * 1e-6);
                _trace.Sample(_model.Time, Reading);
                if (Stopwatch.GetElapsedTime(start).TotalMilliseconds >= 12) break;
            }
            RefreshReading();
            _trace.Invalidate();
        }
        catch (InvalidOperationException e) { Fault(e); }
    }

    private async Task SweepAsync()
    {
        SetRunning(false);
        if (!Voltage) return;
        _trace.Frequency = true;
        _trace.ClearTrace();
        int node = _probes.SelectedIndex;
        var token = _lifetime.Token;
        // Keep the model at the same operating point throughout the sweep.
        foreach (Control panel in Controls) panel.Enabled = false;
        try
        {
            double[] hz = CircuitSimulator.LogFrequencies(1, 100000, 10);
            var amplitudes = await Task.Run(() => hz.Select(f =>
            {
                token.ThrowIfCancellationRequested();
                return CircuitSimulator.GetAcNodeVoltage(_model.SolveAc(f), node).Magnitude;
            }).ToArray(), token);
            if (IsDisposed) return;
            for (int i = 0; i < hz.Length; i++) _trace.Sample(hz[i], amplitudes[i]);
            _trace.Invalidate();
            _reading.Text = "|V(f)|    1 Hz – 100 kHz";
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (InvalidOperationException e) { if (!IsDisposed) Fault(e); }
        finally
        {
            if (!IsDisposed)
                foreach (Control panel in Controls) panel.Enabled = true;
        }
    }

    private void Fault(Exception e)
    {
        SetRunning(false);
        _fault = true;
        _run.Enabled = _sweep.Enabled = false;
        MessageBox.Show(this, e.Message, Text);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            _timer.Dispose();
            _lifetime.Cancel();
            _lifetime.Dispose();
        }
        base.Dispose(disposing);
    }
}

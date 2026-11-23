using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Tools.CircuitSimulation;

internal sealed class TraceView : CanvasControl
{
    private readonly Queue<(double X, double Y)> _points = new();
    private static readonly TextStyle FontStyle = new("Segoe UI", 9);
    private double? _cursorFrequency;
    private RectangleF PlotArea => RectangleF.FromLTRB(78, 16, Width - 18, Height - 35);
    public event Action<double>? FrequencyPicked;
    [System.ComponentModel.DefaultValue("V")]
    internal string Unit { get; set; } = "V";
    [System.ComponentModel.DefaultValue(false)]
    internal bool Frequency { get; set; }

    public void ClearTrace() { _points.Clear(); Invalidate(); }

    public void ShowFrequencyCurve(IEnumerable<(double Frequency, double? Value)> samples)
    {
        Frequency = true;
        _points.Clear();
        foreach (var (frequency, value) in samples)
            _points.Enqueue((frequency, value ?? double.NaN));
        Invalidate();
    }

    public void SetFrequencyCursor(double frequency)
    {
        _cursorFrequency = frequency;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        var area = PlotArea;
        if (e.Button != MouseButtons.Left || FrequencyPicked == null || _points.Count < 2 || !area.Contains(e.Location)) return;
        double fraction = (e.X - area.Left) / area.Width;
        double left = Math.Log10(_points.Peek().X);
        double right = Math.Log10(_points.Last().X);
        FrequencyPicked.Invoke(Math.Pow(10, left + fraction * (right - left)));
    }

    public void Sample(double x, double y)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y)) throw new InvalidOperationException("Non-finite simulation result.");
        if (_points.Count >= 2000) _points.Dequeue();
        _points.Enqueue((x, y));
    }

    protected override void OnRender(CanvasRenderEventArgs e)
    {
        var c = e.Canvas;
        c.Clear(Color.FromArgb(24, 34, 47));
        var area = PlotArea;
        if (area.Width <= 0 || area.Height <= 0) return;
        var grid = new Stroke(Color.FromArgb(65, 79, 95));
        for (int i = 0; i <= 8; i++)
        {
            float x = area.Left + i * area.Width / 8;
            c.DrawLine(new(x, area.Top), new(x, area.Bottom), grid);
        }
        for (int i = 0; i <= 4; i++)
        {
            float y = area.Top + i * area.Height / 4;
            c.DrawLine(new(area.Left, y), new(area.Right, y), grid);
        }
        var data = _points.ToArray();
        if (data.Length < 2) return;
        double X(double t) => Frequency ? Math.Log10(t) : t;
        double left = X(data[0].X), right = X(data[^1].X);
        if (right <= left) return;
        float ChartX(double x) => area.Left + (float)((X(x) - left) / (right - left)) * area.Width;
        var finite = data.Where(p => double.IsFinite(p.Y)).ToArray();
        if (finite.Length > 0)
        {
            double min = finite.Min(p => p.Y), max = finite.Max(p => p.Y);
            bool fixedUnits = Unit is "dB" or "°";
            double pad = Math.Max((max - min) * 0.08, Math.Max(Math.Abs(max) * 0.001, fixedUnits ? 0.1 : 1e-9));
            min -= pad;
            max += pad;
            PointF Position((double X, double Y) p) => new(ChartX(p.X), area.Bottom - (float)((p.Y - min) / (max - min)) * area.Height);
            var pen = new Stroke(Color.Aquamarine, 1.5f);
            for (int i = 1; i < data.Length; i++)
            {
                var previous = data[i - 1];
                var current = data[i];
                if (double.IsFinite(previous.Y) && double.IsFinite(current.Y) &&
                    (Unit != "°" || Math.Abs(current.Y - previous.Y) <= 180))
                    c.DrawLine(Position(previous), Position(current), pen);
            }
            for (int i = 0; i <= 4; i++)
            {
                double value = max - i * (max - min) / 4;
                string label = fixedUnits ? $"{value:0.##} {Unit}" : Units.Format(value, Unit);
                c.DrawText(label, FontStyle, new PointF(3, area.Top - 7 + i * area.Height / 4), Color.WhiteSmoke);
            }
        }
        if (_cursorFrequency is double cursor)
        {
            float x = ChartX(cursor);
            c.DrawLine(new(x, area.Top), new(x, area.Bottom), new Stroke(Color.Gold));
        }
        for (int i = 0; i <= 4; i++)
        {
            double value = left + i * (right - left) / 4;
            c.DrawText(Units.Format(Frequency ? Math.Pow(10,value) : value, Frequency ? "Hz" : "s"), FontStyle,
                new PointF(area.Left - 18 + i * (area.Width - 35) / 4, area.Bottom + 6), Color.WhiteSmoke);
        }
    }
}

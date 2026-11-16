using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Tools.CircuitSimulation;

internal sealed class TraceView : CanvasControl
{
    private readonly Queue<(double X, double Y)> _points = new();
    private static readonly TextStyle FontStyle = new("Segoe UI", 9);
    [System.ComponentModel.DefaultValue("V")]
    internal string Unit { get; set; } = "V";
    [System.ComponentModel.DefaultValue(false)]
    internal bool Frequency { get; set; }

    public void ClearTrace() { _points.Clear(); Invalidate(); }

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
        var area = RectangleF.FromLTRB(78, 16, Width - 18, Height - 35);
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
        double min = data.Min(p => p.Y), max = data.Max(p => p.Y);
        double pad = Math.Max((max - min) * 0.08, Math.Max(Math.Abs(max) * 0.001, 1e-9));
        min -= pad; max += pad;
        double X(double t) => Frequency ? Math.Log10(t) : t;
        double left = X(data[0].X), right = X(data[^1].X);
        PointF Position((double X, double Y) p) => new(
            area.Left + (float)((X(p.X) - left) / (right - left)) * area.Width,
            area.Bottom - (float)((p.Y - min) / (max - min)) * area.Height);
        var pen = new Stroke(Color.Aquamarine, 1.5f);
        for (int i = 1; i < data.Length; i++) c.DrawLine(Position(data[i-1]), Position(data[i]), pen);
        for (int i = 0; i <= 4; i++)
            c.DrawText(Units.Format(max - i * (max-min)/4, Unit), FontStyle,
                new PointF(3, area.Top - 7 + i * area.Height / 4), Color.WhiteSmoke);
        for (int i = 0; i <= 4; i++)
        {
            double value = left + i * (right - left) / 4;
            c.DrawText(Units.Format(Frequency ? Math.Pow(10,value) : value, Frequency ? "Hz" : "s"), FontStyle,
                new PointF(area.Left - 18 + i * (area.Width - 35) / 4, area.Bottom + 6), Color.WhiteSmoke);
        }
    }
}

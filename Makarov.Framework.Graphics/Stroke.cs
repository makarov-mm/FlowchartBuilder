namespace Makarov.Framework.Graphics;

/// <summary>Outline style: the counterpart of a GDI+ pen. Widths are in pixels.</summary>
public readonly record struct Stroke(Color Color, float Width = 1f)
{
    public DashPattern? Dash { get; init; }

    public StrokeJoin Join { get; init; } = StrokeJoin.Miter;

    public StrokeCap Cap { get; init; } = StrokeCap.Flat;

    /// <summary>Whether the default (solid, mitered, flat) Direct2D stroke style can be used.</summary>
    internal bool IsPlain => Dash is null && Join == StrokeJoin.Miter && Cap == StrokeCap.Flat;
}

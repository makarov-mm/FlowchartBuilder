namespace Makarov.FlowchartBuilder.Glyphs;

/// <summary>Terminal in block-local coordinates. The centre is (0,0), and each edge is at ±1.</summary>
public readonly record struct ConnectionPort(PointF Anchor, PointF Direction)
{
    public static readonly ConnectionPort Top = new(new(0, -1), new(0, -1));
    public static readonly ConnectionPort Right = new(new(1, 0), new(1, 0));
    public static readonly ConnectionPort Bottom = new(new(0, 1), new(0, 1));
    public static readonly ConnectionPort Left = new(new(-1, 0), new(-1, 0));
}

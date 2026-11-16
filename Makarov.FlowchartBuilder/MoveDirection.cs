namespace Makarov.FlowchartBuilder;

/// <summary>
/// What a mouse drag does to a block glyph: move it, resize it from a side or a corner, or rotate it.
/// Corner values are combinations of the side flags.
/// </summary>
[Flags]
public enum MoveDirection : byte
{
    None = 0,
    Move = 1,
    Up = 2,
    Down = 4,
    Left = 8,
    Right = 16,
    UpAndLeft = Up | Left,
    UpAndRight = Up | Right,
    DownAndLeft = Down | Left,
    DownAndRight = Down | Right,
    Rotate = 32,
}

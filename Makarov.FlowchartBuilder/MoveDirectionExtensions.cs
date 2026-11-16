namespace Makarov.FlowchartBuilder;

public static class MoveDirectionExtensions
{
    /// <summary>The eight resize directions clockwise, starting from <see cref="MoveDirection.Up"/>.</summary>
    public static IReadOnlyList<MoveDirection> ResizeDirections { get; } =
    [
        MoveDirection.Up,
        MoveDirection.UpAndRight,
        MoveDirection.Right,
        MoveDirection.DownAndRight,
        MoveDirection.Down,
        MoveDirection.DownAndLeft,
        MoveDirection.Left,
        MoveDirection.UpAndLeft,
    ];

    /// <summary>Clockwise angle of a resize direction from "up", in degrees.</summary>
    public static int ToAngle(this MoveDirection direction) => direction switch
    {
        MoveDirection.Up => 0,
        MoveDirection.UpAndRight => 45,
        MoveDirection.Right => 90,
        MoveDirection.DownAndRight => 135,
        MoveDirection.Down => 180,
        MoveDirection.DownAndLeft => 225,
        MoveDirection.Left => 270,
        MoveDirection.UpAndLeft => 315,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Not a resize direction."),
    };

    /// <summary>Angle between two resize directions, in degrees.</summary>
    public static int AngleTo(this MoveDirection direction, MoveDirection other) => direction.ToAngle() - other.ToAngle();
}

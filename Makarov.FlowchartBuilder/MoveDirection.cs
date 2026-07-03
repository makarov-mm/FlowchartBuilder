using System;
using System.Collections.Generic;

namespace Makarov.FlowchartBuilder
{

    [Flags]
    public enum MoveDirection : byte
    {

        None = 0,

        Move = 1,

        Up = 2,

        Down = 4,

        Left = 8,

        Right = 16,

        UpAndLeft = 10,

        UpAndRight = 18,

        DownAndLeft = 12,

        DownAndRight = 20,

        Rotate = 32
    }

    public static class MoveDirectionHelper
    {

        public static int CalculateAngle(this MoveDirection dir1, MoveDirection dir2)
        {
            var angles = new Dictionary<MoveDirection, int>
                             {
                                 {MoveDirection.Up, 0},
                                 {MoveDirection.UpAndRight, 45},
                                 {MoveDirection.Right, 90},
                                 {MoveDirection.DownAndRight, 135},
                                 {MoveDirection.Down, 180},
                                 {MoveDirection.DownAndLeft, 225},
                                 {MoveDirection.Left, 270},
                                 {MoveDirection.UpAndLeft, 315},
                             };

            return angles[dir1] - angles[dir2];
        }

        public static MoveDirection RotateDirection(this MoveDirection source, MoveDirection dir1, MoveDirection dir2)
        {
            var directions = GetDirectionsArray();
            int angle = dir1.CalculateAngle(dir2) / 45;
            if (angle < 0) angle = directions.Length + angle;
            if (angle > directions.Length) angle %= directions.Length;

            int curr = 0;
            for (int i = 0; i < directions.Length; i++)
                if (directions[i] == source)
                {
                    curr = i;
                    break;
                }

            curr += angle;
            if (curr > directions.Length) curr %= directions.Length;
            return directions[curr];
        }

        public static MoveDirection[] GetDirectionsArray()
        {
            return new[]
                       {
                           MoveDirection.Up,
                           MoveDirection.UpAndRight,
                           MoveDirection.Right,
                           MoveDirection.DownAndRight,
                           MoveDirection.Down,
                           MoveDirection.DownAndLeft,
                           MoveDirection.Left,
                           MoveDirection.UpAndLeft
                       };
        }
    }
}

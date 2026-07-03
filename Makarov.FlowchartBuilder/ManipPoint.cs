using System.Drawing;

namespace Makarov.FlowchartBuilder
{

    public struct ManipPoint
    {
        #region Private members

        private PointF _coords;

        private readonly MoveDirection _arrowDirection;

        private readonly MoveDirection _moveDirection;
        #endregion

        #region Constructors

        public ManipPoint(PointF coords, MoveDirection moveDirection, MoveDirection arrowDirection)
        {
            _coords = coords;
            _moveDirection = moveDirection;
            _arrowDirection = arrowDirection;
        }
        #endregion

        #region Properties

        public PointF Coords
        {
            get => _coords;
            set => _coords = value;
        }

        public float XInMM
        {
            get => Coords.X;
            set => Coords = new PointF(value, Coords.Y);
        }

        public float YInMM
        {
            get => Coords.Y;
            set => Coords = new PointF(Coords.X, value);
        }

        public MoveDirection MoveDirection => _moveDirection;

        public MoveDirection ArrowDirection => _arrowDirection;

        #endregion

        #region Operators

        public static explicit operator PointF(ManipPoint pt)
        {
            return pt.Coords;
        }

        public static explicit operator MoveDirection(ManipPoint pt)
        {
            return pt.MoveDirection;
        }
        #endregion
    }
}

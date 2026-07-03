using System;
using System.Drawing;

namespace Makarov.FlowchartBuilder
{

    public struct LinkPoint
    {
        #region Types

        [Flags]
        public enum LinkPointType
        {

            In = 1,

            Out = 2,
        }
        #endregion

        #region Private members

        private readonly LinkPointType _type;

        private PointF _coords;
        #endregion

        #region Constructors

        public LinkPoint(PointF coords)
        {
            _coords = coords;
            _type = LinkPointType.In | LinkPointType.Out;
        }

        public LinkPoint(PointF coords, LinkPointType type)
        {
            _coords = coords;
            _type = type;
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

        public LinkPointType PointType => _type;

        #endregion

        #region Operators

        public static explicit operator PointF(LinkPoint pt)
        {
            return pt.Coords;
        }
        #endregion
    }
}

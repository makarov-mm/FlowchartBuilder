// <copyright file="ManipPoint.cs" company="Michael Makarov">
// Copyright Michael Makarov (c) 2008 All Right Reserved
// </copyright>
// <author>Michael Makarov</author>
// <email>m.m.makarov@gmail.com</email>
// <date>2009-03-20</date>
// <summary>Точка манипулирования.</summary>

using System.Drawing;

namespace Makarov.FlowchartBuilder
{
    /// <summary>
    /// Точка манипулирования.
    /// </summary>
    public struct ManipPoint
    {
        #region Private members
        /// <summary>
        /// Координаты в миллиметрах.
        /// </summary>
        private PointF _coords;

        /// <summary>
        /// Направление стрелки.
        /// </summary>
        private readonly MoveDirection _arrowDirection;

        /// <summary>
        /// Направление движения.
        /// </summary>
        private readonly MoveDirection _moveDirection;
        #endregion

        #region Constructors
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="coords">Координаты в миллиметрах.</param>
        /// <param name="moveDirection">Направление движения.</param>
        /// <param name="arrowDirection">Направление стрелки.</param>
        public ManipPoint(PointF coords, MoveDirection moveDirection, MoveDirection arrowDirection)
        {
            _coords = coords;
            _moveDirection = moveDirection;
            _arrowDirection = arrowDirection;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Координаты в миллиметрах.
        /// </summary>
        public PointF Coords
        {
            get => _coords;
            set => _coords = value;
        }

        /// <summary>
        /// Координата X в миллиметрах.
        /// </summary>
        public float XInMM
        {
            get => Coords.X;
            set => Coords = new PointF(value, Coords.Y);
        }

        /// <summary>
        /// Координата Y в миллиметрах.
        /// </summary>
        public float YInMM
        {
            get => Coords.Y;
            set => Coords = new PointF(Coords.X, value);
        }

        /// <summary>
        /// Направление движения.
        /// </summary>
        public MoveDirection MoveDirection => _moveDirection;

        /// <summary>
        /// Направление стрелки.
        /// </summary>
        public MoveDirection ArrowDirection => _arrowDirection;

        #endregion

        #region Operators
        /// <summary>
        /// Преобразование в точку.
        /// </summary>
        public static explicit operator PointF(ManipPoint pt)
        {
            return pt.Coords;
        }

        /// <summary>
        /// Преобразование в направление движения.
        /// </summary>
        public static explicit operator MoveDirection(ManipPoint pt)
        {
            return pt.MoveDirection;
        }
        #endregion
    }
}
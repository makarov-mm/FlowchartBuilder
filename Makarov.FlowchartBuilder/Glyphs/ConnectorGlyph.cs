using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs
{

    public class ConnectorGlyph : LinkGlyph
    {
        #region Private members

        private BlockGlyph _source;

        private BlockGlyph _target;
        #endregion

        #region Constructors

        public ConnectorGlyph()
        {
            Caption = string.Empty;
        }

        public ConnectorGlyph(BlockGlyph source, int sourcePort, BlockGlyph target, int targetPort)
        {
            Caption = string.Empty;
            _source = source;
            _target = target;
            SourcePort = sourcePort;
            TargetPort = targetPort;
            SourceGlyphId = source == null ? null : source.Id;
            TargetGlyphId = target == null ? null : target.Id;
        }
        #endregion

        #region Properties

        public BlockGlyph Source
        {
            get { return _source; }

            set
            {
                _source = value;
                SourceGlyphId = value == null ? null : value.Id;
            }
        }

        public BlockGlyph Target
        {
            get { return _target; }

            set
            {
                _target = value;
                TargetGlyphId = value == null ? null : value.Id;
            }
        }

        [ActiveProperty]
        public string SourceGlyphId { get; set; }

        [ActiveProperty]
        public string TargetGlyphId { get; set; }

        [ActiveProperty]
        public int SourcePort { get; set; }

        [ActiveProperty]
        public int TargetPort { get; set; }

        public bool IsResolved
        {
            get { return _source != null && _target != null; }
        }
        #endregion

        #region Public methods

        public bool Connects(AbstractGlyph glyph)
        {
            return glyph != null && (ReferenceEquals(glyph, _source) || ReferenceEquals(glyph, _target));
        }

        public void Relink(IEnumerable<AbstractGlyph> glyphs)
        {
            _source = null;
            _target = null;

            foreach (AbstractGlyph glyph in glyphs)
            {
                var block = glyph as BlockGlyph;

                if (block == null)
                    continue;

                if (_source == null && block.Id == SourceGlyphId)
                    _source = block;

                if (_target == null && block.Id == TargetGlyphId)
                    _target = block;
            }
        }

        public bool HitTest(PointF pt, float tolerance)
        {
            if (!IsResolved)
                return false;

            return DistanceToSegment(pt, SourcePoint, TargetPoint) <= tolerance;
        }
        #endregion

        #region Geometry

        private static int ClampPort(int port)
        {
            if (port < 0)
                return 0;

            if (port > 3)
                return 3;

            return port;
        }

        private PointF SourcePoint
        {
            get { return _source.LinkPointsScaled[ClampPort(SourcePort)]; }
        }

        private PointF TargetPoint
        {
            get { return _target.LinkPointsScaled[ClampPort(TargetPort)]; }
        }

        private static float DistanceToSegment(PointF p, PointF a, PointF b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            float lengthSquared = dx * dx + dy * dy;

            if (lengthSquared <= 0.0001f)
            {
                float ax = p.X - a.X;
                float ay = p.Y - a.Y;
                return (float)Math.Sqrt(ax * ax + ay * ay);
            }

            float t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSquared;

            if (t < 0f)
                t = 0f;
            else if (t > 1f)
                t = 1f;

            float projX = a.X + t * dx;
            float projY = a.Y + t * dy;
            float ex = p.X - projX;
            float ey = p.Y - projY;

            return (float)Math.Sqrt(ex * ex + ey * ey);
        }
        #endregion

        #region Draw methods

        protected override void DrawGlyph(Graphics gfx)
        {
            if (!IsResolved)
                return;

            PointF from = SourcePoint;
            PointF to = TargetPoint;

            using (var pen = new Pen(Selected ? Color.DodgerBlue : Color.Black, Selected ? 2f : 1.5f))
            {
                pen.StartCap = LineCap.Round;

                using (var cap = new AdjustableArrowCap(4f, 5f, true))
                {
                    pen.CustomEndCap = cap;
                    gfx.DrawLine(pen, from, to);
                }
            }
        }

        protected override void DrawSelectedGlyph(Graphics gfx)
        {
            DrawGlyph(gfx);
        }
        #endregion
    }
}

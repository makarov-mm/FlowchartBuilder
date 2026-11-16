using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.Framework.Core;

namespace Makarov.FlowchartBuilder.Glyphs
{

    [GlyphDefaultWidth(30f)]
    [GlyphDefaultHeight(20f)]
    [GlyphMinimalWidth(15f)]
    [GlyphMinimalHeight(15f)]
    public class BlockGlyph : AbstractGlyph
    {

        #region Constructors

        protected BlockGlyph(int x, int y, int width, int height)
        {
            XInMM = x;
            YInMM = y;
            WidthInMM = width;
            HeightInMM = height;

            Selected = false;
            Caption = Name;
            ForegroundColor = Color.Black;
            BorderColor = Color.Black;
            BorderWidthInMM = 0.5f;
            TextFont = "Arial".SafeCreateFont(8);
        }

        protected BlockGlyph()
        {
            var range = new Range(50, 100);
            XInMM = RandomGenerator.Instance.Next(range);
            YInMM = RandomGenerator.Instance.Next(range);
            WidthInMM = DefaultWidthInMM;
            HeightInMM = DefaultHeightInMM;

            Selected = false;
            Caption = Name;
            ForegroundColor = Color.Black;
            BorderColor = Color.Black;
            BorderWidthInMM = 0.5f;
            TextFont = "Arial".SafeCreateFont(8);
        }
        #endregion

        #region Private members

        private float _angle;

        private float _width;

        private float _height;
        #endregion

        #region Minimal size properties

        protected float MinimalWidthInMM
        {
            get
            {
                return ((GlyphMinimalWidthAttribute)(GetType().GetCustomAttributes(typeof(GlyphMinimalWidthAttribute), true)[0])).WidthInMM;
            }
        }

        protected int MinimalWidth
        {
            get { return UnitsConverter.MMToPx(MinimalWidthInMM); }
        }

        protected float MinimalWidthInInches
        {
            get { return UnitsConverter.MMToInch(MinimalWidthInMM); }
        }

        protected float MinimalWidthInCurr
        {
            get { return UnitsConverter.MMToCurr(MinimalWidthInMM); }
        }

        protected float MinimalHeightInMM
        {
            get
            {
                return ((GlyphMinimalHeightAttribute)(GetType().GetCustomAttributes(typeof(GlyphMinimalHeightAttribute), true)[0])).HeightInMM;
            }
        }

        protected int MinimalHeight
        {
            get { return UnitsConverter.MMToPx(MinimalHeightInMM); }
        }

        protected float MinimalHeightInInches
        {
            get { return UnitsConverter.MMToInch(MinimalHeightInMM); }
        }

        protected float MinimalHeightInCurr
        {
            get { return UnitsConverter.MMToCurr(MinimalHeightInMM); }
        }
        #endregion

        #region InnerRegion

        protected virtual Rectangle InnerRegion
        {
            get
            {
                return new Rectangle(
                    ScaledX - (ScaledWidth >> 1),
                    ScaledY - (ScaledHeight >> 1),
                    ScaledWidth,
                    ScaledHeight);
            }
        }
        #endregion

        #region Properties

        public float ScaleFactor
        {
            get { return Core.Instance.CurrentDocument.DocumentSheet.ScaleFactor; }
        }

        public ContainerGlyph Owner
        {
            get;
            set;
        }

        [Description(@"Text font.")]
        [Category(@"Appearance")]
        [DisplayName(@"Font")]
        [ActiveProperty]
        public Font TextFont
        {
            get;
            set;
        }

        public int BorderWidth
        {
            get { return BorderWidthInMM.MMToPx(); }
            set { BorderWidthInMM = value.PxToMM(); }
        }

        public int ScaledBorderWidth
        {
            get { return (BorderWidthInMM * ScaleFactor).MMToPx(); }
            set { BorderWidthInMM = value.PxToMM() / ScaleFactor; }
        }

        [Description(@"Border width in mm.")]
        [Category(@"Appearance")]
        [DisplayName(@"Border Width")]
        [ActiveProperty]
        public float BorderWidthInMM
        {
            get; set;
        }

        [Description(@"Border color.")]
        [Category(@"Appearance")]
        [DisplayName(@"Border Color")]
        [ActiveProperty]
        public Color BorderColor
        {
            get; set;
        }

        [Description(@"Background glyph color.")]
        [Category(@"Appearance")]
        [DisplayName(@"Background Color")]
        [ActiveProperty]
        public Color BackgroundColor
        {
            get; set;
        }

        [Description(@"Foreground glyph color.")]
        [Category(@"Appearance")]
        [DisplayName(@"Foreground Color")]
        [ActiveProperty]
        public Color ForegroundColor
        {
            get; set;
        }

        public bool InGroup
        {
            get { return Group != null; }
        }

        public List<BlockGlyph> Group
        {
            get
            {
                return Core.Instance.CurrentDocument.DocumentSheet.Groups.FirstOrDefault(group => group.Contains(this));
            }
        }

        public float GroupLeft
        {
            get
            {

                if (!InGroup)
                    throw new InvalidContextException();

                return Group.Min(g => g.XInMM - g.WidthInMM / 2);
            }
        }

        public float GroupRight
        {
            get
            {

                if (!InGroup)
                    throw new InvalidContextException();

                return Group.Max(g => g.XInMM + g.WidthInMM / 2);
            }
        }

        public float GroupTop
        {
            get
            {

                if (!InGroup)
                    throw new InvalidContextException();

                return Group.Min(g => g.YInMM - g.HeightInMM / 2);
            }
        }

        public float GroupBottom
        {
            get
            {

                if (!InGroup)
                    throw new InvalidContextException();

                return Group.Max(g => g.YInMM + g.HeightInMM / 2);
            }
        }

        public float GroupX
        {
            get { return (GroupRight + GroupLeft) / 2; }

            set
            {

                float dx = value - GroupX;

                foreach (var glyph in Group)
                    glyph.XInMM += dx;
            }
        }

        public float GroupY
        {
            get { return (GroupBottom + GroupTop) / 2; }

            set
            {

                float dy = value - GroupY;

                foreach (var glyph in Group)
                    glyph.YInMM += dy;
            }
        }

        public PointF GroupCenter
        {
            get { return new PointF(GroupX, GroupY); }

            set
            {
                GroupX = value.X;
                GroupY = value.Y;
            }
        }

        public float GroupWidth
        {
            get { return GroupRight - GroupLeft; }
        }

        public float GroupHeight
        {
            get { return GroupBottom - GroupTop; }
        }
        #endregion

        #region Manipulation properties

        public MoveDirection Direction
        {
            get;
            set;
        }

        public float DirectionsAngle
        {
            get;
            set;
        }

        public virtual float ManipulationPointSize
        {
            get { return 7f; }
        }

        public virtual float RotationPointSize
        {
            get { return ManipulationPointSize * 1.5f; }
        }

        public virtual float RotationPointOffset
        {
            get { return 40f; }
        }

        public PointF[] RotationPoints
        {
            get
            {
                float angle = Angle.DegToRad();
                var cos = (float) System.Math.Cos(angle);
                var sin = (float) System.Math.Sin(angle);

                var startPoint = new PointF(
                    ScaledX,
                    ScaledY - (ScaledHeight >> 1));

                var pt0 = new PointF(
                    startPoint.X,
                    startPoint.Y - ManipulationPointSize / 2f);

                var endPoint = new PointF(
                    ScaledX,
                    ScaledY - (ScaledHeight >> 1) - RotationPointOffset);

                var pt1 = new PointF(
                    endPoint.X,
                    endPoint.Y + RotationPointSize/2f);

                var point0 = new PointF(
                    (startPoint.X - ScaledX) * cos - (startPoint.Y - ScaledY) * sin + ScaledX,
                    (startPoint.Y - ScaledY) * cos + (startPoint.X - ScaledX) * sin + ScaledY);

                var point1 = new PointF(
                    (pt0.X - ScaledX) * cos - (pt0.Y - ScaledY) * sin + ScaledX,
                    (pt0.Y - ScaledY) * cos + (pt0.X - ScaledX) * sin + ScaledY);

                var point2 = new PointF(
                    (pt1.X - ScaledX) * cos - (pt1.Y - ScaledY) * sin + ScaledX,
                    (pt1.Y - ScaledY) * cos + (pt1.X - ScaledX) * sin + ScaledY);

                var point3 = new PointF(
                    (endPoint.X - ScaledX) * cos - (endPoint.Y - ScaledY) * sin + ScaledX,
                    (endPoint.Y - ScaledY) * cos + (endPoint.X - ScaledX) * sin + ScaledY);

                return new[] { point0, point1, point2, point3 };
            }
        }

        public virtual IEnumerable<ManipPoint> ManipulationPoints
        {
            get
            {
                float halfScaledWidth = ScaledWidth/2f;
                float halfScaledHeight = ScaledHeight/2f;

                var pts = new[]
                              {

                                  new PointF(
                                      ScaledX,
                                      ScaledY - halfScaledHeight),

                                  new PointF(
                                      ScaledX + halfScaledWidth,
                                      ScaledY - halfScaledHeight),

                                  new PointF(
                                      ScaledX + halfScaledWidth,
                                      ScaledY),

                                  new PointF(
                                      ScaledX + halfScaledWidth,
                                      ScaledY + halfScaledHeight),

                                  new PointF(
                                      ScaledX,
                                      ScaledY + halfScaledHeight),

                                  new PointF(
                                      ScaledX - halfScaledWidth,
                                      ScaledY + halfScaledHeight),

                                  new PointF(
                                      ScaledX - halfScaledWidth,
                                      ScaledY),

                                  new PointF(
                                      ScaledX - halfScaledWidth,
                                      ScaledY - halfScaledHeight)
                              };

                var directions = MoveDirectionHelper.GetDirectionsArray();

                float angle = Angle;
                float radians = angle.DegToRad();
                var cos = (float) System.Math.Cos(radians);
                var sin = (float) System.Math.Sin(radians);

                var directionOffset = angle > 0
                    ? (int)System.Math.Truncate((angle + (45 / 2f)) / 45f)
                    : (int)System.Math.Truncate((angle - (45 / 2f)) / 45f);

                for (int i = 0; i < pts.Length; i++)
                {
                    PointF pt = pts[i];

                    int j = i + directionOffset;
                    if (j < 0) j = directions.Length + j;
                    if (j >= directions.Length) j %= directions.Length;
                    MoveDirection direction = directions[j];

                    yield return new ManipPoint(
                        new PointF(
                            (pt.X - ScaledX) * cos - (pt.Y - ScaledY) * sin + ScaledX,
                            (pt.Y - ScaledY) * cos + (pt.X - ScaledX) * sin + ScaledY),
                        directions[i],
                        direction);
                }
            }
        }
        #endregion

        #region Link properties

        public virtual float LinkPointSize
        {
            get { return 7f; }
        }

        public virtual IEnumerable<LinkPoint> LinkPoints
        {
            get { return new List<LinkPoint>(); }
        }
        #endregion

        #region Default values properties

        protected float DefaultXInMM
        {
            get
            {
                var sheet = Core.Instance.CurrentDocument.DocumentSheet;
                return (sheet.ViewOrigin.X + Core.Instance.MainWindow.InnerRectangle.Width / (2 * sheet.ScaleFactor)).PxToMM();
            }
        }

        protected int DefaultX
        {
            get { return UnitsConverter.MMToPx(DefaultXInMM); }
        }

        protected float DefaultXInInches
        {
            get { return UnitsConverter.MMToInch(DefaultXInMM); }
        }

        protected float DefaultXInCurr
        {
            get { return UnitsConverter.MMToCurr(DefaultXInMM); }
        }

        protected float DefaultYInMM
        {
            get
            {
                var sheet = Core.Instance.CurrentDocument.DocumentSheet;
                return (sheet.ViewOrigin.Y + Core.Instance.MainWindow.InnerRectangle.Height / (2 * sheet.ScaleFactor)).PxToMM();
            }
        }

        protected int DefaultY
        {
            get { return UnitsConverter.MMToPx(DefaultYInMM); }
        }

        protected float DefaultYInInches
        {
            get { return UnitsConverter.MMToInch(DefaultYInMM); }
        }

        protected float DefaultYInCurr
        {
            get { return UnitsConverter.MMToCurr(DefaultYInMM); }
        }

        public float DefaultWidthInMM
        {
            get
            {
                return ((GlyphDefaultWidthAttribute)(GetType().GetCustomAttributes(typeof(GlyphDefaultWidthAttribute), true)[0])).WidthInMM;
            }
        }

        protected int DefaultWidth
        {
            get { return UnitsConverter.MMToPx(DefaultWidthInMM); }
        }

        protected float DefaultWidthInInches
        {
            get { return UnitsConverter.MMToInch(DefaultWidthInMM); }
        }

        protected float DefaultWidthInCurr
        {
            get { return UnitsConverter.MMToCurr(DefaultWidthInMM); }
        }

        public float DefaultHeightInMM
        {
            get
            {
                return ((GlyphDefaultHeightAttribute)(GetType().GetCustomAttributes(typeof(GlyphDefaultHeightAttribute), true)[0])).HeightInMM;
            }
        }

        protected int DefaultHeight
        {
            get { return UnitsConverter.MMToPx(DefaultHeightInMM); }
        }

        protected float DefaultHeightInInches
        {
            get { return UnitsConverter.MMToInch(DefaultHeightInMM); }
        }

        protected float DefaultHeightInCurr
        {
            get { return UnitsConverter.MMToCurr(DefaultHeightInMM); }
        }
        #endregion

        #region Draw methods

        protected override void DrawGlyph(Graphics gfx)
        {
            using (Brush brush = new SolidBrush(Color.FromArgb(10, Color.Blue)))
            {
                gfx.FillRectangle(
                    brush,
                    ScaledX - (ScaledWidth >> 1), ScaledY - (ScaledHeight >> 1),
                    ScaledWidth, ScaledHeight);
            }

            using (var pen = new Pen(Color.Silver))
            {
                pen.DashPattern = new float[] { 4, 2 };
                gfx.DrawRectangle(
                    pen,
                    ScaledX - (ScaledWidth >> 1), ScaledY - (ScaledHeight >> 1),
                    ScaledWidth, ScaledHeight);
            }

            const int crossWidth = 40;

            gfx.DrawLine(
                Pens.Silver,
                ScaledX - (crossWidth >> 2), ScaledY,
                ScaledX + (crossWidth >> 2), ScaledY);

            gfx.DrawLine(
                Pens.Silver,
                ScaledX, ScaledY - (crossWidth >> 2),
                ScaledX, ScaledY + (crossWidth >> 2));
        }

        protected override void DrawText(Graphics gfx)
        {
            using (var font = "Arial".SafeCreateFont(8))
            using (var brush = new SolidBrush(ForegroundColor))
                gfx.DrawString(Caption, font, brush, InnerRegion);
        }

        protected override void DrawDots(Graphics gfx)
        {
            Pen pen;
            Brush brush;

            if (InGroup)
            {
                pen = Settings.Pens.GroupSelectionBorder;
                brush = Settings.Brushes.GroupSelection;
            }
            else
            {
                pen = Settings.Pens.SelectionBorder;
                brush = Settings.Brushes.Selection;
            }

            var halfManipulationPointSize = (float)System.Math.Ceiling(ManipulationPointSize / 2f);
            float angle = Angle;

            foreach (var pt in ManipulationPoints)
            {
                Matrix mx = gfx.Transform;

                gfx.TranslateTransform(pt.Coords.X, pt.Coords.Y);
                gfx.RotateTransform(angle);
                gfx.TranslateTransform(-pt.Coords.X, -pt.Coords.Y);

                gfx.FillRectangle(
                    brush,
                    (int) (pt.Coords.X - halfManipulationPointSize),
                    (int) (pt.Coords.Y - halfManipulationPointSize),
                    (int) ManipulationPointSize,
                    (int) ManipulationPointSize);

                gfx.DrawRectangle(
                    pen,
                    (int) (pt.Coords.X - halfManipulationPointSize),
                    (int) (pt.Coords.Y - halfManipulationPointSize),
                    (int) ManipulationPointSize,
                    (int) ManipulationPointSize);

                gfx.Transform = mx;
            }

            if (CanRotate)
            {
                var pts = RotationPoints;
                float halfRotationPointSize = RotationPointSize / 2f;

                gfx.DrawEllipse(
                    pen,
                    pts[3].X - halfRotationPointSize, pts[3].Y - halfRotationPointSize,
                    RotationPointSize, RotationPointSize);

                gfx.FillEllipse(
                    brush,
                    pts[3].X - halfRotationPointSize, pts[3].Y - halfRotationPointSize,
                    RotationPointSize, RotationPointSize);

                using (var dashPen = (Pen) pen.Clone())
                {
                    dashPen.DashPattern = new[] {3f, 3f};
                    gfx.DrawLine(
                        dashPen,
                        pts[1].X, pts[1].Y,
                        pts[2].X, pts[2].Y);
                }
            }
        }

        protected override void DrawLinkPoints(Graphics gfx)
        {
            if (Core.Instance.CurrGlyphTypeSelected &&
                Core.Instance.CurrentGlyphType.IsSubclassOf(typeof(LinkGlyph)))
            {
                float halfLinkPointSize = LinkPointSize / 2f;

                foreach (var pt in LinkPoints)
                {
                    gfx.FillPie(
                        Settings.Brushes.Selection,
                        pt.Coords.X - halfLinkPointSize,
                        pt.Coords.Y - halfLinkPointSize,
                        LinkPointSize,
                        LinkPointSize,
                        0f, 360f);

                    gfx.DrawPie(
                        Settings.Pens.SelectionBorder,
                        pt.Coords.X - halfLinkPointSize,
                        pt.Coords.Y - halfLinkPointSize,
                        LinkPointSize,
                        LinkPointSize,
                        0f, 360f);
                }
            }
        }

        protected override void DrawHelpers(Graphics gfx)
        {
            Color color = InGroup
                              ? Settings.Colors.GroupSelectionBorder
                              : Settings.Colors.SelectionBorder;

            using (Brush brush = new SolidBrush(color))
                if (Settings.Environment.SizeLocationLabel && Direction != MoveDirection.None)
                    if (Direction == MoveDirection.Move)
                    {
                        using (var font = "Arial".SafeCreateFont(7))
                            gfx.DrawString(
                                string.Format("[x:{0}{2}, y:{1}{2}]",
                                              XInMM.MMToCurr().ToString("F1"),
                                              YInMM.MMToCurr().ToString("F1"),
                                              Core.Instance.CurrentTranslator[
                                                  Settings.Environment.CurrentUnits.ShortName]),
                                font,
                                brush,
                                ScaledX - (ScaledWidth >> 1) - ManipulationPointSize,
                                ScaledY + (ScaledHeight >> 1) + ManipulationPointSize);
                    }
                    else
                    {
                        using (var font = "Arial".SafeCreateFont(7))
                            gfx.DrawString(
                                string.Format("[{0}:{1}{4}, {2}:{3}{4}]",
                                              Core.Instance.CurrentTranslator["width"],
                                              WidthInMM.MMToCurr().ToString("F1"),
                                              Core.Instance.CurrentTranslator["height"],
                                              HeightInMM.MMToCurr().ToString("F1"),
                                              Core.Instance.CurrentTranslator[
                                                  Settings.Environment.CurrentUnits.ShortName]),
                                font,
                                brush,
                                ScaledX - (ScaledWidth >> 1) - ManipulationPointSize,
                                ScaledY + (ScaledHeight >> 1) + ManipulationPointSize);
                    }
        }
        #endregion

        #region Public methods

        public bool InRegion(Rectangle rect)
        {
            int ix = ScaledX - (ScaledWidth >> 1);
            int iy = ScaledY - (ScaledHeight >> 1);

            if (ix + ScaledWidth < rect.X) return false;
            if (ix > rect.X + rect.Width) return false;

            if (iy + ScaledHeight < rect.Y) return false;
            if (iy > rect.Y + rect.Height) return false;

            return true;
        }
        #endregion

        #region ISize

        public int Width
        {
            get { return WidthInMM.MMToPx(); }
            set { WidthInMM = value.PxToMM(); }
        }

        public int Height
        {
            get { return HeightInMM.MMToPx(); }
            set { HeightInMM = value.PxToMM(); }
        }

        [Description(@"Width in mm.")]
        [Category(@"Layout")]
        [DisplayName(@"Width")]
        [ActiveProperty]
        public float WidthInMM
        {
            get { return _width; }

            set
            {
                if (value < MinimalWidthInMM)
                    value = MinimalWidthInMM;

                if (value <= 0)
                    throw new InvalidValueException(@"Sheet.Width", value.ToString());

                _width = value;
            }
        }

        [Description(@"Height in mm.")]
        [Category(@"Layout")]
        [DisplayName(@"Height")]
        [ActiveProperty]
        public float HeightInMM
        {
            get { return _height; }

            set
            {
                if (value < MinimalHeightInMM)
                    value = MinimalHeightInMM;

                if (value <= 0)
                    throw new InvalidValueException(@"Sheet.Height", value.ToString());

                _height = value;
            }
        }

        public float WidthInInches
        {
            get { return WidthInMM.MMToInch(); }
            set { WidthInMM = value.InchToMM(); }
        }

        public float HeightInInches
        {
            get { return HeightInMM.MMToInch(); }
            set { HeightInMM = value.InchToMM(); }
        }

        public float WidthInCurr
        {
            get { return WidthInMM.MMToCurr(); }
            set { WidthInMM = value.CurrToMM(); }
        }

        public float HeightInCurr
        {
            get { return HeightInMM.MMToCurr(); }
            set { HeightInMM = value.CurrToMM(); }
        }
        #endregion

        #region IScaledSize

        public int ScaledWidth
        {
            get { return Width.Scale(); }
            set { Width = value.Unscale(); }
        }

        public int ScaledHeight
        {
            get { return Height.Scale(); }
            set { Height = value.Unscale(); }
        }

        public float ScaledWidthInMM
        {
            get { return WidthInMM.Scale(); }
            set { WidthInMM = value.Unscale(); }
        }

        public float ScaledHeightInMM
        {
            get { return HeightInMM.Scale(); }
            set { HeightInMM = value.Unscale(); }
        }

        public float ScaledWidthInInches
        {
            get { return WidthInInches.Scale(); }
            set { WidthInInches = value.Unscale(); }
        }

        public float ScaledHeightInInches
        {
            get { return HeightInInches.Scale(); }
            set { HeightInInches = value.Unscale(); }
        }

        public float ScaledWidthInCurr
        {
            get { return WidthInCurr.Scale(); }
            set { WidthInCurr = value.Unscale(); }
        }

        public float ScaledHeightInCurr
        {
            get { return HeightInCurr.Scale(); }
            set { HeightInCurr = value.Unscale(); }
        }
        #endregion

        #region ILocation

        public int X
        {
            get { return XInMM.MMToPx(); }
            set { XInMM = value.PxToMM(); }
        }

        public int Y
        {
            get { return YInMM.MMToPx(); }
            set { YInMM = value.PxToMM(); }
        }

        [Description(@"X coordinate in mm.")]
        [Category(@"Layout")]
        [DisplayName(@"X")]
        [ActiveProperty]
        public float XInMM { get; set; }

        [Description(@"Y coordinate in mm.")]
        [Category(@"Layout")]
        [DisplayName(@"Y")]
        [ActiveProperty]
        public float YInMM { get; set; }

        public float XInInches
        {
            get { return XInMM.MMToInch(); }
            set { XInMM = value.InchToMM(); }
        }

        public float YInInches
        {
            get { return YInMM.MMToInch(); }
            set { YInMM = value.InchToMM(); }
        }

        public float XInCurr
        {
            get { return XInMM.MMToCurr(); }
            set { XInMM = value.CurrToMM(); }
        }

        public float YInCurr
        {
            get { return YInMM.MMToCurr(); }
            set { YInMM = value.CurrToMM(); }
        }
        #endregion

        #region IScaledLocation

        public int ScaledX
        {
            get { return X.Scale(); }
            set { X = value.Unscale(); }
        }

        public int ScaledY
        {
            get { return Y.Scale(); }
            set { Y = value.Unscale(); }
        }

        public float ScaledXInMM
        {
            get { return XInMM.Scale(); }
            set { XInMM = value.Unscale(); }
        }

        public float ScaledYInMM
        {
            get { return YInMM.Scale(); }
            set { YInMM = value.Unscale(); }
        }

        public float ScaledXInInches
        {
            get { return XInInches.Scale(); }
            set { XInInches = value.Unscale(); }
        }

        public float ScaledYInInches
        {
            get { return YInInches.Scale(); }
            set { YInInches = value.Unscale(); }
        }

        public float ScaledXInCurr
        {
            get { return XInCurr.Scale(); }
            set { XInCurr = value.Unscale(); }
        }

        public float ScaledYInCurr
        {
            get { return YInCurr.Scale(); }
            set { YInCurr = value.Unscale(); }
        }
        #endregion

        #region Rotation

        [Description(@"Angle.")]
        [Category(@"Layout")]
        [DisplayName(@"Angle")]
        [ActiveProperty]
        public float Angle
        {
            get
            {
                if (System.Math.Abs(_angle) > 360)
                    _angle = (float)(_angle - System.Math.Truncate(_angle / 360) * 360);

                return _angle;
            }

            set
            {
                _angle = value;

                if (System.Math.Abs(_angle) > 360)
                    _angle = (float)(_angle - System.Math.Truncate(_angle / 360) * 360);
            }
        }

        public virtual bool CanRotate
        {
            get
            {

                return true;
            }
        }
        #endregion

        #region Connection points

        public virtual bool SupportsLinks
        {
            get { return false; }
        }

        public PointF[] LinkPointsScaled
        {
            get
            {
                int halfWidth = ScaledWidth >> 1;
                int halfHeight = ScaledHeight >> 1;

                return new[]
                {
                    new PointF(ScaledX, ScaledY - halfHeight),
                    new PointF(ScaledX + halfWidth, ScaledY),
                    new PointF(ScaledX, ScaledY + halfHeight),
                    new PointF(ScaledX - halfWidth, ScaledY)
                };
            }
        }
        #endregion

    }
}

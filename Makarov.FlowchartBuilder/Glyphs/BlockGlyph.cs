using Math = System.Math;
using System.ComponentModel;
using System.Globalization;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.Framework.Core;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs;

/// <summary>
/// Rectangular glyph positioned by its center. It can be moved, resized, rotated, grouped and, if
/// <see cref="SupportsLinks"/>, connected to other block glyphs. Sizes and positions are stored in millimeters;
/// the Scaled* members are screen pixels at the current zoom.
/// </summary>
[GlyphDefaultWidth(30f)]
[GlyphDefaultHeight(20f)]
[GlyphMinimalWidth(15f)]
[GlyphMinimalHeight(15f)]
public class BlockGlyph : AbstractGlyph
{
    private const int CenterMarkSize = 10;
    private const int LockMarkerOffset = 2;

    private static readonly Color PlaceholderFill = Color.FromArgb(10, Color.Blue);
    private static readonly Stroke PlaceholderStroke = new(Color.Silver) { Dash = new DashPattern(4f, 2f) };
    private static readonly Stroke CenterMarkStroke = new(Color.Silver);
    private static readonly TextStyle LabelStyle = new("Arial", 7f);
    private static readonly Color LockColor = Color.FromArgb(255, 80, 80, 80);
    private static readonly Color LockFill = Color.FromArgb(255, 250, 215, 90);

    private float _width;
    private float _height;
    private float _angle;

    protected BlockGlyph()
    {
        XInMM = Random.Shared.Next(50, 101);
        YInMM = Random.Shared.Next(50, 101);
        WidthInMM = Metadata.DefaultWidthInMM;
        HeightInMM = Metadata.DefaultHeightInMM;

        var translator = Core.Instance.CurrentTranslator;
        Caption = translator.Contains(Name) ? translator[Name] : Name;
        ForegroundColor = Color.Black;
        BorderColor = Color.Black;
        BorderWidthInMM = 0.5f;
        TextFont = Fonts.Create("Arial", 8);
    }

    /// <summary>Zoom of the current document.</summary>
    public float ScaleFactor => Core.Instance.CurrentDocument.DocumentSheet.ScaleFactor;

    #region Appearance

    [Description("Text font.")]
    [Category("Appearance")]
    [DisplayName("Font")]
    [ActiveProperty]
    public Font TextFont { get; set; }

    [Description("Border width in mm.")]
    [Category("Appearance")]
    [DisplayName("Border Width")]
    [ActiveProperty]
    public float BorderWidthInMM { get; set; }

    /// <summary>Border width in pixels at the current zoom; at least one pixel.</summary>
    public float ScaledBorderWidth => Math.Max(1f, UnitsConverter.MMToPxExact(BorderWidthInMM * ScaleFactor));

    [Description("Border color.")]
    [Category("Appearance")]
    [DisplayName("Border Color")]
    [ActiveProperty]
    public Color BorderColor { get; set; }

    [Description("Background glyph color.")]
    [Category("Appearance")]
    [DisplayName("Background Color")]
    [ActiveProperty]
    public Color BackgroundColor { get; set; }

    [Description("Foreground glyph color.")]
    [Category("Appearance")]
    [DisplayName("Foreground Color")]
    [ActiveProperty]
    public Color ForegroundColor { get; set; }

    #endregion

    #region Size and position

    [Description("Width in mm.")]
    [Category("Layout")]
    [DisplayName("Width")]
    [ActiveProperty]
    public float WidthInMM
    {
        get => _width;
        set
        {
            value = Math.Max(value, Metadata.MinimalWidthInMM);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            _width = value;
        }
    }

    [Description("Height in mm.")]
    [Category("Layout")]
    [DisplayName("Height")]
    [ActiveProperty]
    public float HeightInMM
    {
        get => _height;
        set
        {
            value = Math.Max(value, Metadata.MinimalHeightInMM);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            _height = value;
        }
    }

    [Description("X coordinate in mm.")]
    [Category("Layout")]
    [DisplayName("X")]
    [ActiveProperty]
    public float XInMM { get; set; }

    [Description("Y coordinate in mm.")]
    [Category("Layout")]
    [DisplayName("Y")]
    [ActiveProperty]
    public float YInMM { get; set; }

    /// <summary>Rotation in degrees, kept within [-360, 360].</summary>
    [Description("Angle.")]
    [Category("Layout")]
    [DisplayName("Angle")]
    [ActiveProperty]
    public float Angle
    {
        get => _angle;
        set
        {
            if (!CanRotate)
                value = 0f;

            _angle = Math.Abs(value) > 360 ? (float)(value - Math.Truncate(value / 360.0) * 360) : value;
        }
    }

    public int Width => WidthInMM.MMToPx();

    public int Height => HeightInMM.MMToPx();

    public int X => XInMM.MMToPx();

    public int Y => YInMM.MMToPx();

    public int ScaledWidth => Width.Scale();

    public int ScaledHeight => Height.Scale();

    public int ScaledX
    {
        get => X.Scale();
        set => XInMM = value.Unscale().PxToMM();
    }

    public int ScaledY
    {
        get => Y.Scale();
        set => YInMM = value.Unscale().PxToMM();
    }

    public float ScaledXInMM
    {
        get => XInMM.Scale();
        set => XInMM = value.Unscale();
    }

    public float ScaledYInMM
    {
        get => YInMM.Scale();
        set => YInMM = value.Unscale();
    }

    public float ScaledWidthInMM
    {
        get => WidthInMM.Scale();
        set => WidthInMM = value.Unscale();
    }

    public float ScaledHeightInMM
    {
        get => HeightInMM.Scale();
        set => HeightInMM = value.Unscale();
    }

    /// <summary>Bounds in sheet pixels at the current zoom, ignoring rotation.</summary>
    public Rectangle ScaledBounds =>
        new(ScaledX - (ScaledWidth >> 1), ScaledY - (ScaledHeight >> 1), ScaledWidth, ScaledHeight);

    /// <summary>Bounds in millimeters, ignoring rotation.</summary>
    public RectangleF BoundsInMM => new(XInMM - WidthInMM / 2, YInMM - HeightInMM / 2, WidthInMM, HeightInMM);

    /// <summary>Area the caption is drawn into.</summary>
    protected virtual Rectangle InnerRegion => ScaledBounds;

    #endregion

    #region Grouping

    /// <summary>The group this glyph belongs to in the current document, if any.</summary>
    public List<BlockGlyph>? Group =>
        Core.Instance.CurrentDocument.DocumentSheet.Groups.FirstOrDefault(group => group.Contains(this));

    public bool InGroup => Group is not null;

    #endregion

    #region Manipulation

    /// <summary>What the current mouse drag does to the glyph.</summary>
    public MoveDirection Direction { get; set; }

    /// <summary>Angle between the dragged handle and the side it resizes, used to project the mouse movement.</summary>
    public float DirectionsAngle { get; set; }

    public virtual bool CanRotate => true;

    public virtual float ManipulationPointSize => 7f;

    public virtual float RotationPointSize => ManipulationPointSize * 1.5f;

    public virtual float RotationPointOffset => 40f;

    /// <summary>
    /// The rotation handle stem, rotated with the glyph: [0] top edge center, [1] stem start,
    /// [2] stem end and [3] handle center.
    /// </summary>
    public PointF[] RotationPoints
    {
        get
        {
            float top = ScaledY - (ScaledHeight >> 1);
            float handle = top - RotationPointOffset;

            return
            [
                RotateAroundCenter(new PointF(ScaledX, top)),
                RotateAroundCenter(new PointF(ScaledX, top - ManipulationPointSize / 2f)),
                RotateAroundCenter(new PointF(ScaledX, handle + RotationPointSize / 2f)),
                RotateAroundCenter(new PointF(ScaledX, handle)),
            ];
        }
    }

    /// <summary>The eight resize handles clockwise from the top center, rotated with the glyph.</summary>
    public virtual IEnumerable<ManipPoint> ManipulationPoints
    {
        get
        {
            float halfWidth = ScaledWidth / 2f;
            float halfHeight = ScaledHeight / 2f;

            PointF[] points =
            [
                new(ScaledX, ScaledY - halfHeight),
                new(ScaledX + halfWidth, ScaledY - halfHeight),
                new(ScaledX + halfWidth, ScaledY),
                new(ScaledX + halfWidth, ScaledY + halfHeight),
                new(ScaledX, ScaledY + halfHeight),
                new(ScaledX - halfWidth, ScaledY + halfHeight),
                new(ScaledX - halfWidth, ScaledY),
                new(ScaledX - halfWidth, ScaledY - halfHeight),
            ];

            // After rotation by more than half a step, a handle visually points to the next direction.
            float angle = Angle;
            int directionOffset = (int)MathF.Truncate((angle > 0 ? angle + 22.5f : angle - 22.5f) / 45f);
            IReadOnlyList<MoveDirection> directions = MoveDirectionExtensions.ResizeDirections;

            for (int i = 0; i < points.Length; i++)
            {
                int j = ((i + directionOffset) % directions.Count + directions.Count) % directions.Count;
                yield return new ManipPoint(RotateAroundCenter(points[i]), directions[i], directions[j]);
            }
        }
    }

    #endregion

    #region Links

    public virtual bool SupportsLinks => false;

    public virtual float LinkPointSize => 7f;

    /// <summary>Connection ports in sheet pixels: [0] top, [1] right, [2] bottom, [3] left.</summary>
    public PointF[] LinkPointsScaled
    {
        get
        {
            int halfWidth = ScaledWidth >> 1;
            int halfHeight = ScaledHeight >> 1;

            return
            [
                new(ScaledX, ScaledY - halfHeight),
                new(ScaledX + halfWidth, ScaledY),
                new(ScaledX, ScaledY + halfHeight),
                new(ScaledX - halfWidth, ScaledY),
            ];
        }
    }

    #endregion

    /// <summary>Whether the glyph bounds touch <paramref name="rect"/> (sheet pixels).</summary>
    public bool InRegion(Rectangle rect)
    {
        Rectangle bounds = ScaledBounds;

        return bounds.Right >= rect.X && bounds.X <= rect.Right
            && bounds.Bottom >= rect.Y && bounds.Y <= rect.Bottom;
    }

    /// <summary>Hit test against the unrotated bounds, edges included (sheet pixels).</summary>
    public bool Contains(Point point)
    {
        int halfWidth = ScaledWidth >> 1;
        int halfHeight = ScaledHeight >> 1;

        return point.X >= ScaledX - halfWidth && point.X <= ScaledX + halfWidth
            && point.Y >= ScaledY - halfHeight && point.Y <= ScaledY + halfHeight;
    }

    #region Drawing

    protected internal override bool IsPropertyVisible(string propertyName) =>
        propertyName != nameof(Angle) || CanRotate;

    protected override CanvasScope ApplyRotation(Canvas canvas) =>
        CanRotate && Angle != 0f ? canvas.Rotate(Angle, new PointF(ScaledX, ScaledY)) : default;

    protected void DrawShadow(Canvas canvas, Geometry outline)
    {
        using (canvas.Translate(3f * ScaleFactor, 3f * ScaleFactor))
            canvas.FillGeometry(outline, Color.FromArgb(54, Color.Black));
    }

    public virtual bool PreparePlacement(IWin32Window owner) => true;

    /// <summary>Placeholder look for block glyphs that do not draw themselves.</summary>
    protected override void DrawGlyph(Canvas canvas)
    {
        Rectangle bounds = ScaledBounds;
        canvas.FillRectangle(bounds, PlaceholderFill);
        canvas.DrawRectangle(bounds, PlaceholderStroke);

        canvas.DrawLine(new PointF(ScaledX - CenterMarkSize, ScaledY), new PointF(ScaledX + CenterMarkSize, ScaledY), CenterMarkStroke);
        canvas.DrawLine(new PointF(ScaledX, ScaledY - CenterMarkSize), new PointF(ScaledX, ScaledY + CenterMarkSize), CenterMarkStroke);
    }

    /// <summary>The caption inside <see cref="InnerRegion"/>, scaled with the zoom.</summary>
    protected override void DrawText(Canvas canvas) =>
        canvas.DrawText(Caption, TextStyle.FromFont(TextFont).Scale(ScaleFactor), InnerRegion, ForegroundColor);

    protected override void DrawDots(Canvas canvas)
    {
        bool inGroup = InGroup;
        Stroke stroke = inGroup ? Settings.Strokes.GroupSelectionBorder : Settings.Strokes.SelectionBorder;
        Color fill = inGroup ? Settings.Colors.GroupSelection : Settings.Colors.Selection;

        float halfSize = MathF.Ceiling(ManipulationPointSize / 2f);
        var size = (int)ManipulationPointSize;

        foreach (ManipPoint point in ManipulationPoints)
        {
            using (canvas.Rotate(Angle, point.Coords))
            {
                var square = new Rectangle((int)(point.Coords.X - halfSize), (int)(point.Coords.Y - halfSize), size, size);
                canvas.FillRectangle(square, fill);
                canvas.DrawRectangle(square, stroke);
            }
        }

        if (!CanRotate)
            return;

        PointF[] stem = RotationPoints;
        float radius = RotationPointSize / 2f;
        var handle = new RectangleF(stem[3].X - radius, stem[3].Y - radius, RotationPointSize, RotationPointSize);

        canvas.FillEllipse(handle, fill);
        canvas.DrawEllipse(handle, stroke);
        canvas.DrawLine(stem[1], stem[2], stroke with { Dash = new DashPattern(3f, 3f) });
    }

    protected override void DrawHelpers(Canvas canvas)
    {
        if (!Settings.Environment.SizeLocationLabel || Direction == MoveDirection.None)
            return;

        Translator translator = Core.Instance.CurrentTranslator;
        string units = translator[Settings.Environment.CurrentUnits.ShortName];

        string label = Direction == MoveDirection.Move
            ? $"[x:{Format(XInMM)}{units}, y:{Format(YInMM)}{units}]"
            : $"[{translator["width"]}:{Format(WidthInMM)}{units}, {translator["height"]}:{Format(HeightInMM)}{units}]";

        canvas.DrawText(
            label,
            LabelStyle,
            new PointF(ScaledX - (ScaledWidth >> 1) - ManipulationPointSize, ScaledY + (ScaledHeight >> 1) + ManipulationPointSize),
            InGroup ? Settings.Colors.GroupSelectionBorder : Settings.Colors.SelectionBorder);

        static string Format(float mm) => mm.MMToCurr().ToString("F1", CultureInfo.CurrentCulture);
    }

    /// <summary>Padlock in the top-right corner of a fixed glyph.</summary>
    protected override void DrawLockMarker(Canvas canvas)
    {
        float right = ScaledX + (ScaledWidth >> 1) - LockMarkerOffset;
        float top = ScaledY - (ScaledHeight >> 1) + LockMarkerOffset;
        var body = new RectangleF(right - 10f, top + 5f, 10f, 7f);
        var stroke = new Stroke(LockColor, 1.5f);

        using Geometry shackle = new GeometryBuilder()
            .BeginFigure(new PointF(body.Left + 2f, body.Top), filled: false)
            .Arc(new RectangleF(body.Left + 2f, top, 6f, 6f), 180f, 180f)
            .LineTo(new PointF(body.Right - 2f, body.Top))
            .EndFigure(closed: false)
            .ToGeometry();

        canvas.DrawGeometry(shackle, stroke);
        canvas.FillRectangle(body, LockFill);
        canvas.DrawRectangle(body, stroke with { Width = 1f });
    }

    #endregion

    private PointF RotateAroundCenter(PointF point)
    {
        float radians = float.DegreesToRadians(Angle);
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        float dx = point.X - ScaledX;
        float dy = point.Y - ScaledY;

        return new PointF(dx * cos - dy * sin + ScaledX, dy * cos + dx * sin + ScaledY);
    }
}

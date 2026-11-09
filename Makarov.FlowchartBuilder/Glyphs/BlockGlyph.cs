using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Globalization;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.Framework.Core;

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

    private static readonly Brush PlaceholderBrush = new SolidBrush(Color.FromArgb(10, Color.Blue));
    private static readonly Pen PlaceholderPen = new(Color.Silver) { DashPattern = [4f, 2f] };
    private static readonly Brush SelectionLabelBrush = new SolidBrush(Settings.Colors.SelectionBorder);
    private static readonly Brush GroupSelectionLabelBrush = new SolidBrush(Settings.Colors.GroupSelectionBorder);
    private static Font? _labelFont;

    private float _width;
    private float _height;
    private float _angle;

    protected BlockGlyph()
    {
        XInMM = Random.Shared.Next(50, 101);
        YInMM = Random.Shared.Next(50, 101);
        WidthInMM = Metadata.DefaultWidthInMM;
        HeightInMM = Metadata.DefaultHeightInMM;

        Caption = Name;
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

    public int ScaledBorderWidth => (BorderWidthInMM * ScaleFactor).MMToPx();

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
        set => _angle = Math.Abs(value) > 360 ? (float)(value - Math.Truncate(value / 360.0) * 360) : value;
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

    protected override void ApplyRotation(Graphics graphics)
    {
        if (!CanRotate || Angle == 0)
            return;

        graphics.TranslateTransform(ScaledX, ScaledY);
        graphics.RotateTransform(Angle);
        graphics.TranslateTransform(-ScaledX, -ScaledY);
    }

    /// <summary>Placeholder look for block glyphs that do not draw themselves.</summary>
    protected override void DrawGlyph(Graphics graphics)
    {
        Rectangle bounds = ScaledBounds;
        graphics.FillRectangle(PlaceholderBrush, bounds);
        graphics.DrawRectangle(PlaceholderPen, bounds);

        graphics.DrawLine(Pens.Silver, ScaledX - CenterMarkSize, ScaledY, ScaledX + CenterMarkSize, ScaledY);
        graphics.DrawLine(Pens.Silver, ScaledX, ScaledY - CenterMarkSize, ScaledX, ScaledY + CenterMarkSize);
    }

    protected override void DrawText(Graphics graphics)
    {
        using var brush = new SolidBrush(ForegroundColor);
        graphics.DrawString(Caption, TextFont, brush, InnerRegion);
    }

    protected override void DrawDots(Graphics graphics)
    {
        bool inGroup = InGroup;
        Pen pen = inGroup ? Settings.Pens.GroupSelectionBorder : Settings.Pens.SelectionBorder;
        Brush brush = inGroup ? Settings.Brushes.GroupSelection : Settings.Brushes.Selection;

        float halfSize = MathF.Ceiling(ManipulationPointSize / 2f);
        var size = (int)ManipulationPointSize;

        foreach (ManipPoint point in ManipulationPoints)
        {
            GraphicsState state = graphics.Save();
            graphics.TranslateTransform(point.Coords.X, point.Coords.Y);
            graphics.RotateTransform(Angle);
            graphics.TranslateTransform(-point.Coords.X, -point.Coords.Y);

            var square = new Rectangle((int)(point.Coords.X - halfSize), (int)(point.Coords.Y - halfSize), size, size);
            graphics.FillRectangle(brush, square);
            graphics.DrawRectangle(pen, square);

            graphics.Restore(state);
        }

        if (!CanRotate)
            return;

        PointF[] stem = RotationPoints;
        float radius = RotationPointSize / 2f;
        var handle = new RectangleF(stem[3].X - radius, stem[3].Y - radius, RotationPointSize, RotationPointSize);

        graphics.DrawEllipse(pen, handle);
        graphics.FillEllipse(brush, handle);

        using var dashPen = (Pen)pen.Clone();
        dashPen.DashPattern = [3f, 3f];
        graphics.DrawLine(dashPen, stem[1], stem[2]);
    }

    protected override void DrawHelpers(Graphics graphics)
    {
        if (!Settings.Environment.SizeLocationLabel || Direction == MoveDirection.None)
            return;

        Translator translator = Core.Instance.CurrentTranslator;
        string units = translator[Settings.Environment.CurrentUnits.ShortName];

        string label = Direction == MoveDirection.Move
            ? $"[x:{Format(XInMM)}{units}, y:{Format(YInMM)}{units}]"
            : $"[{translator["width"]}:{Format(WidthInMM)}{units}, {translator["height"]}:{Format(HeightInMM)}{units}]";

        graphics.DrawString(
            label,
            _labelFont ??= Fonts.Create("Arial", 7),
            InGroup ? GroupSelectionLabelBrush : SelectionLabelBrush,
            ScaledX - (ScaledWidth >> 1) - ManipulationPointSize,
            ScaledY + (ScaledHeight >> 1) + ManipulationPointSize);

        static string Format(float mm) => mm.MMToCurr().ToString("F1", CultureInfo.CurrentCulture);
    }

    /// <summary>Lock icon in the top-right corner of a fixed glyph.</summary>
    protected override void DrawLockMarker(Graphics graphics)
    {
        Bitmap image = Core.Instance.Images["Lock"];

        graphics.DrawImageUnscaled(
            image,
            ScaledX + (ScaledWidth >> 1) - image.Width - LockMarkerOffset,
            ScaledY - (ScaledHeight >> 1) + LockMarkerOffset);
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

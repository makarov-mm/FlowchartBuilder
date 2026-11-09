using System.Reflection;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Sheets;

/// <summary>
/// Sheet with a paper size; the default size comes from <see cref="SheetWidthAttribute"/> and <see cref="SheetHeightAttribute"/>.
/// </summary>
[SheetWidth(210)]
[SheetHeight(297)]
public abstract class FixedSheet : Sheet
{
    private float _width;
    private float _height;

    protected FixedSheet()
    {
        Width = DefaultWidthInMM;
        Height = DefaultHeightInMM;
    }

    public float DefaultWidthInMM => GetType().GetCustomAttribute<SheetWidthAttribute>()!.WidthInMM;

    public float DefaultHeightInMM => GetType().GetCustomAttribute<SheetHeightAttribute>()!.HeightInMM;

    /// <summary>Paper width in millimeters.</summary>
    public float Width
    {
        get => _width;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            _width = value;
        }
    }

    /// <summary>Paper height in millimeters.</summary>
    public float Height
    {
        get => _height;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            _height = value;
        }
    }

    protected override void DrawThumbnail(Canvas canvas)
    {
        const int innerOffset = 8;
        const int shadowOffset = 2;

        int innerWidth = ThumbnailWidth - innerOffset * 2;
        int innerHeight = ThumbnailHeight - innerOffset * 2;

        // All paper formats share one scale (500 mm per thumbnail), so their sizes compare visually.
        float width = DefaultWidthInMM / 500f * innerWidth;
        float height = DefaultHeightInMM / 500f * innerHeight;
        float left = (innerWidth - width) / 2f + innerOffset;
        float top = (innerHeight - height) / 2f + innerOffset;
        var paper = new RectangleF(left, top, width, height);

        canvas.FillRectangle(new RectangleF(0, 0, ThumbnailWidth, ThumbnailHeight), Color.White);
        canvas.FillRectangle(new RectangleF(left + shadowOffset, top + shadowOffset, width, height), Settings.Colors.ThumbnailBorder);
        canvas.FillRectangle(paper, new LinearGradient(paper.Location, new PointF(paper.Right, paper.Bottom), Settings.Colors.SheetGradient, Color.White));
        canvas.DrawRectangle(paper, Settings.Strokes.ThumbnailBorder);
        DrawThumbnailBorder(canvas);
    }

    /// <summary>Zoom at which the whole paper fits into <paramref name="fraction"/> of the viewport.</summary>
    public float GetFitScaleFactor(Size viewport, float fraction)
    {
        float horizontal = Width.MMToPx() / (viewport.Width * fraction);
        float vertical = Height.MMToPx() / (viewport.Height * fraction);
        return 1 / Math.Max(horizontal, vertical);
    }

    public override Size GetPixelSize(bool scaled, Size viewport)
    {
        float scale = scaled ? ScaleFactor : 1f;
        return new Size((Width * scale).MMToPx(), (Height * scale).MMToPx());
    }
}

using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Flowchart;

/// <summary>
/// Flowchart block: a gradient-filled outline with four link ports that connectors attach to.
/// </summary>
[GlyphFamily("Flowchart")]
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(30)]
public abstract class AbstractFlowchartGlyph : BlockGlyph
{
    private static readonly Brush LinkPointBrush = new SolidBrush(Color.White);
    private static readonly Pen LinkPointPen = new(Color.SteelBlue);

    protected AbstractFlowchartGlyph()
    {
        BackgroundColor = Color.Silver;
        ForegroundColor = Color.WhiteSmoke;
    }

    public override bool CanRotate => false;

    public override bool SupportsLinks => true;

    public override Bitmap Thumbnail
    {
        get
        {
            Bitmap bitmap = base.Thumbnail;

            using Graphics graphics = Graphics.FromImage(bitmap);
            using GraphicsPath path = CreateThumbnailOutline();
            using var brush = new LinearGradientBrush(
                Point.Empty,
                new Point(ThumbnailWidth, ThumbnailHeight),
                Settings.Colors.ThumbnailObjectGradientStart,
                Settings.Colors.ThumbnailObjectGradientEnd);

            graphics.FillPath(brush, path);
            graphics.DrawPath(Settings.Pens.ThumbnailBorder, path);
            DrawThumbnailDetails(graphics, path.GetBounds());
            DrawThumbnailBorder(graphics);

            return bitmap;
        }
    }

    /// <summary>Vertical margin of the block in the palette image.</summary>
    protected virtual int ThumbnailInsetY => ThumbnailHeight >> 2;

    /// <summary>Creates the outline fitted into the given box.</summary>
    protected abstract Primitive CreatePrimitive(int x, int y, int width, int height);

    /// <summary>Outline on the sheet, centered on the glyph position.</summary>
    protected virtual GraphicsPath CreateOutline() =>
        CreatePrimitive(ScaledX, ScaledY, ScaledWidth, ScaledHeight).CreateCentered();

    /// <summary>Outline in the palette image.</summary>
    protected virtual GraphicsPath CreateThumbnailOutline()
    {
        int insetX = ThumbnailWidth >> 3;
        int insetY = ThumbnailInsetY;
        return CreatePrimitive(insetX, insetY, ThumbnailWidth - (insetX << 1), ThumbnailHeight - (insetY << 1)).Create();
    }

    protected override void DrawGlyph(Graphics graphics)
    {
        using GraphicsPath path = CreateOutline();
        using var brush = new LinearGradientBrush(
            new PointF(ScaledX - ScaledWidth / 2, ScaledY - ScaledHeight / 2),
            new PointF(ScaledX + ScaledWidth / 2, ScaledY + ScaledHeight / 2),
            ForegroundColor,
            BackgroundColor);
        using var pen = new Pen(BorderColor, ScaledBorderWidth);

        graphics.FillPath(brush, path);
        graphics.DrawPath(pen, path);
        DrawDetails(graphics, pen);
    }

    /// <summary>Extra lines inside the outline, drawn with the border pen.</summary>
    protected virtual void DrawDetails(Graphics graphics, Pen pen)
    {
    }

    /// <summary>Extra lines inside the palette image outline.</summary>
    protected virtual void DrawThumbnailDetails(Graphics graphics, RectangleF bounds)
    {
    }

    protected override void DrawText(Graphics graphics)
    {
    }

    protected override void DrawLinkPoints(Graphics graphics)
    {
        float size = LinkPointSize;
        float half = size / 2f;

        foreach (PointF point in LinkPointsScaled)
        {
            graphics.FillEllipse(LinkPointBrush, point.X - half, point.Y - half, size, size);
            graphics.DrawEllipse(LinkPointPen, point.X - half, point.Y - half, size, size);
        }
    }
}

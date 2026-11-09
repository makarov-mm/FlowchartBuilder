using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Shapes;

/// <summary>
/// A filled geometric shape without a caption; the outline comes from a <see cref="Primitive"/>.
/// </summary>
[GlyphFamily("Shapes")]
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(30)]
public abstract class AbstractShapeGlyph : BlockGlyph
{
    protected AbstractShapeGlyph()
    {
        BackgroundColor = Color.Yellow;
    }

    public override Bitmap Thumbnail
    {
        get
        {
            Bitmap bitmap = base.Thumbnail;

            using Graphics graphics = Graphics.FromImage(bitmap);
            using GraphicsPath path = CreateThumbnailPrimitive().Create();
            using var brush = new LinearGradientBrush(
                Point.Empty,
                new Point(ThumbnailWidth, ThumbnailHeight),
                Settings.Colors.ThumbnailObjectGradientStart,
                Settings.Colors.ThumbnailObjectGradientEnd);

            graphics.FillPath(brush, path);
            graphics.DrawPath(Settings.Pens.ThumbnailBorder, path);
            DrawThumbnailBorder(graphics);

            return bitmap;
        }
    }

    /// <summary>Vertical margin of the shape in the palette image; flat shapes use a larger one.</summary>
    protected virtual int ThumbnailInsetY => ThumbnailHeight >> 3;

    /// <summary>Creates the outline fitted into the given box.</summary>
    protected abstract Primitive CreatePrimitive(int x, int y, int width, int height);

    protected override void DrawGlyph(Graphics graphics)
    {
        using GraphicsPath path = CreatePrimitive(ScaledX, ScaledY, ScaledWidth, ScaledHeight).CreateCentered();
        using var brush = new SolidBrush(BackgroundColor);
        using var pen = new Pen(BorderColor, ScaledBorderWidth);

        graphics.FillPath(brush, path);
        graphics.DrawPath(pen, path);
    }

    protected override void DrawText(Graphics graphics)
    {
    }

    private Primitive CreateThumbnailPrimitive()
    {
        int insetX = ThumbnailWidth >> 3;
        int insetY = ThumbnailInsetY;
        return CreatePrimitive(insetX, insetY, ThumbnailWidth - (insetX << 1), ThumbnailHeight - (insetY << 1));
    }
}

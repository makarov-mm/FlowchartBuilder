using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;
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

    /// <summary>Vertical margin of the shape in the palette image; flat shapes use a larger one.</summary>
    protected virtual int ThumbnailInsetY => ThumbnailHeight >> 3;

    /// <summary>Creates the outline fitted into the given box.</summary>
    protected abstract Primitive CreatePrimitive(int x, int y, int width, int height);

    protected override void DrawThumbnail(Canvas canvas)
    {
        base.DrawThumbnail(canvas);

        using Geometry outline = CreateThumbnailPrimitive().Create();
        canvas.FillGeometry(
            outline,
            new LinearGradient(
                PointF.Empty,
                new PointF(ThumbnailWidth, ThumbnailHeight),
                Settings.Colors.ThumbnailObjectGradientStart,
                Settings.Colors.ThumbnailObjectGradientEnd));
        canvas.DrawGeometry(outline, Settings.Strokes.ThumbnailBorder);
        DrawThumbnailBorder(canvas);
    }

    protected override void DrawGlyph(Canvas canvas)
    {
        using Geometry outline = CreatePrimitive(ScaledX, ScaledY, ScaledWidth, ScaledHeight).CreateCentered();
        canvas.FillGeometry(outline, BackgroundColor);
        canvas.DrawGeometry(outline, new Stroke(BorderColor, ScaledBorderWidth));
    }

    protected override void DrawText(Canvas canvas)
    {
    }

    private Primitive CreateThumbnailPrimitive()
    {
        int insetX = ThumbnailWidth >> 3;
        int insetY = ThumbnailInsetY;
        return CreatePrimitive(insetX, insetY, ThumbnailWidth - (insetX << 1), ThumbnailHeight - (insetY << 1));
    }
}

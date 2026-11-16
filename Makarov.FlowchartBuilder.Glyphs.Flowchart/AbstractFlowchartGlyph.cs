using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;
using Makarov.Framework.Graphics.Primitives;

namespace Makarov.FlowchartBuilder.Glyphs.Flowchart;

/// <summary>
/// Flowchart block: a gradient-filled outline with a centered caption and four link ports that connectors attach to.
/// Flowchart blocks are never rotated.
/// </summary>
[GlyphFamily("Flowchart")]
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(30)]
public abstract class AbstractFlowchartGlyph : BlockGlyph
{
    protected AbstractFlowchartGlyph()
    {
        BackgroundColor = Color.Silver;
        ForegroundColor = Color.WhiteSmoke;
    }

    public override bool CanRotate => false;

    public override bool SupportsLinks => true;

    /// <summary>Vertical margin of the block in the palette image.</summary>
    protected virtual int ThumbnailInsetY => ThumbnailHeight >> 2;

    /// <summary>Part of the block the caption is fitted into.</summary>
    protected virtual RectangleF TextRegion
    {
        get
        {
            RectangleF bounds = ScaledBounds;
            bounds.Inflate(-bounds.Width * 0.08f, -bounds.Height * 0.08f);
            return bounds;
        }
    }

    /// <summary>Creates the outline fitted into the given box.</summary>
    protected abstract Primitive CreatePrimitive(int x, int y, int width, int height);

    /// <summary>Outline on the sheet, centered on the glyph position.</summary>
    protected virtual Geometry CreateOutline() =>
        CreatePrimitive(ScaledX, ScaledY, ScaledWidth, ScaledHeight).CreateCentered();

    /// <summary>Outline in the palette image.</summary>
    protected virtual Geometry CreateThumbnailOutline()
    {
        int insetX = ThumbnailWidth >> 3;
        int insetY = ThumbnailInsetY;
        return CreatePrimitive(insetX, insetY, ThumbnailWidth - (insetX << 1), ThumbnailHeight - (insetY << 1)).Create();
    }

    protected override void DrawThumbnail(Canvas canvas)
    {
        base.DrawThumbnail(canvas);

        using Geometry outline = CreateThumbnailOutline();
        canvas.FillGeometry(
            outline,
            new LinearGradient(
                PointF.Empty,
                new PointF(ThumbnailWidth, ThumbnailHeight),
                Settings.Colors.ThumbnailObjectGradientStart,
                Settings.Colors.ThumbnailObjectGradientEnd));
        canvas.DrawGeometry(outline, Settings.Strokes.ThumbnailBorder);
        DrawThumbnailDetails(canvas, outline.Bounds);
        DrawThumbnailBorder(canvas);
    }

    protected override void DrawGlyph(Canvas canvas)
    {
        using Geometry outline = CreateOutline();
        DrawShadow(canvas, outline);
        var stroke = new Stroke(BorderColor, ScaledBorderWidth);

        canvas.FillGeometry(
            outline,
            new LinearGradient(
                new PointF(ScaledX - ScaledWidth / 2f, ScaledY - ScaledHeight / 2f),
                new PointF(ScaledX + ScaledWidth / 2f, ScaledY + ScaledHeight / 2f),
                ForegroundColor,
                BackgroundColor));
        canvas.DrawGeometry(outline, stroke);
        DrawDetails(canvas, stroke);
    }

    /// <summary>Extra lines inside the outline, drawn with the border stroke.</summary>
    protected virtual void DrawDetails(Canvas canvas, Stroke stroke)
    {
    }

    /// <summary>Extra lines inside the palette image outline.</summary>
    protected virtual void DrawThumbnailDetails(Canvas canvas, RectangleF bounds)
    {
    }

    /// <summary>The caption centered in the block, in the border color, scaled with the zoom.</summary>
    protected override void DrawText(Canvas canvas) =>
        canvas.DrawText(
            Caption,
            TextStyle.FromFont(TextFont).Scale(ScaleFactor),
            TextRegion,
            BorderColor,
            TextAlignment.Center,
            ParagraphAlignment.Center);
}

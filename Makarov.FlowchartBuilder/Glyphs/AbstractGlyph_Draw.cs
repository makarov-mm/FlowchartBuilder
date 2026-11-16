using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs;

public abstract partial class AbstractGlyph
{
    /// <summary>
    /// Draws the glyph in sheet pixel coordinates; the canvas transform is left as it was found. Without
    /// <paramref name="adornments"/> the editing marks (handles, link points, lock markers) are left out.
    /// </summary>
    public void Draw(Canvas canvas, bool adornments = true)
    {
        using (ApplyRotation(canvas))
        {
            if (Selected)
                DrawSelectedGlyph(canvas);
            else
                DrawGlyph(canvas);
        }

        DrawText(canvas);

        if (!adornments)
            return;

        if (Selected)
        {
            DrawDots(canvas);
            DrawHelpers(canvas);
        }
        else
        {
            DrawLinkPoints(canvas);
        }

        if (Fixed)
            DrawLockMarker(canvas);
    }

    /// <summary>Rotates the outline (not the text or the handles) of glyphs that support rotation.</summary>
    protected virtual CanvasScope ApplyRotation(Canvas canvas) => default;

    protected virtual void DrawGlyph(Canvas canvas) { }

    protected virtual void DrawSelectedGlyph(Canvas canvas) => DrawGlyph(canvas);

    protected virtual void DrawText(Canvas canvas) { }

    /// <summary>Selection handles.</summary>
    protected virtual void DrawDots(Canvas canvas) { }

    protected virtual void DrawLinkPoints(Canvas canvas) { }

    /// <summary>Size and position hints shown while the glyph is dragged.</summary>
    protected virtual void DrawHelpers(Canvas canvas) { }

    protected virtual void DrawLockMarker(Canvas canvas) { }
}

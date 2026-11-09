using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Makarov.FlowchartBuilder.Glyphs;

public abstract partial class AbstractGlyph
{
    /// <summary>Draws the glyph in sheet pixel coordinates; the graphics state is left as it was found.</summary>
    public void Draw(Graphics graphics)
    {
        GraphicsState state = graphics.Save();
        graphics.SmoothingMode = SmoothMode;
        ApplyRotation(graphics);

        if (Selected)
            DrawSelectedGlyph(graphics);
        else
            DrawGlyph(graphics);

        graphics.Restore(state);

        state = graphics.Save();
        graphics.TextRenderingHint = Settings.Environment.Antialiasing
            ? TextRenderingHint.ClearTypeGridFit
            : TextRenderingHint.SystemDefault;
        DrawText(graphics);
        graphics.Restore(state);

        if (Selected)
        {
            state = graphics.Save();
            graphics.SmoothingMode = SmoothMode;
            DrawDots(graphics);
            graphics.Restore(state);

            DrawHelpers(graphics);
        }
        else
        {
            DrawLinkPoints(graphics);
        }

        if (Fixed)
            DrawLockMarker(graphics);
    }

    /// <summary>Rotates the outline (not the text or the handles) of glyphs that support rotation.</summary>
    protected virtual void ApplyRotation(Graphics graphics) { }

    protected virtual void DrawGlyph(Graphics graphics) { }

    protected virtual void DrawSelectedGlyph(Graphics graphics) => DrawGlyph(graphics);

    protected virtual void DrawText(Graphics graphics) { }

    /// <summary>Selection handles.</summary>
    protected virtual void DrawDots(Graphics graphics) { }

    protected virtual void DrawLinkPoints(Graphics graphics) { }

    /// <summary>Size and position hints shown while the glyph is dragged.</summary>
    protected virtual void DrawHelpers(Graphics graphics) { }

    protected virtual void DrawLockMarker(Graphics graphics) { }
}

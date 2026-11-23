using System.ComponentModel;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;
using RoundedRectangle = Makarov.Framework.Graphics.Primitives.RoundedRectangle;

namespace Makarov.FlowchartBuilder.Glyphs.States;

[GlyphFamily("States"), GlyphName("State"), GlyphOrder(1)]
[GlyphDefaultWidth(46), GlyphDefaultHeight(21), GlyphMinimalWidth(24), GlyphMinimalHeight(13)]
public sealed class StateGlyph : BlockGlyph
{
    public StateGlyph()
    {
        Caption = "State1";
        BackgroundColor = Color.White;
        BorderWidthInMM = 0.4f;
    }

    public override bool CanRotate => false;
    public override bool SupportsLinks => true;
    public override Type DefaultConnectorType => typeof(TransitionGlyph);

    [ActiveProperty, Category("States"), DisplayName("Initial state")]
    public bool IsInitial { get; set; }

    [ActiveProperty, Category("States"), DisplayName("Final state")]
    public bool IsFinal { get; set; }

    [Browsable(false)]
    public bool IsCurrent { get; set; }

    public override bool PreparePlacement(IWin32Window owner)
    {
        var names = Core.Instance.ActiveSheet?.Glyphs.OfType<StateGlyph>().Select(state => state.Caption)
            .ToHashSet(StringComparer.Ordinal) ?? [];
        int number = 1;
        while (names.Contains($"State{number}")) number++;
        Caption = $"State{number}";
        IsInitial = names.Count == 0;
        return true;
    }

    protected override void DrawGlyph(Canvas canvas)
    {
        Rectangle bounds = ScaledBounds;
        using var outline = Outline(bounds);
        canvas.FillGeometry(outline, BackgroundColor);
        var border = new Stroke(BorderColor, ScaledBorderWidth);
        canvas.DrawGeometry(outline, border);
        RectangleF text = DrawMarkers(canvas, bounds, border, ScaleFactor, IsInitial, IsFinal);
        canvas.DrawText(Caption, TextStyle.FromFont(TextFont).Scale(ScaleFactor), text, ForegroundColor,
            TextAlignment.Center, ParagraphAlignment.Center);
    }

    protected override void DrawText(Canvas canvas) { }

    protected override void DrawAdornment(Canvas canvas)
    {
        if (!IsCurrent) return;
        var bounds = ScaledBounds;
        bounds.Inflate(4, 4);
        using var outline = Outline(bounds);
        canvas.DrawGeometry(outline, new Stroke(Color.White, 5));
        canvas.DrawGeometry(outline, new Stroke(Color.FromArgb(0, 95, 175), 3));
    }

    protected override void DrawThumbnail(Canvas canvas)
    {
        base.DrawThumbnail(canvas);
        var bounds = new Rectangle(4, 11, ThumbnailWidth - 8, ThumbnailHeight - 22);
        using var outline = Outline(bounds);
        var border = new Stroke(Settings.Colors.ThumbnailBorder, 1.3f);
        canvas.FillGeometry(outline, Color.White);
        canvas.DrawGeometry(outline, border);
        var text = DrawMarkers(canvas, bounds, border, 0.75f, IsInitial, IsFinal);
        canvas.DrawText("S", new TextStyle("Arial", 8f), text, border.Color, TextAlignment.Center, ParagraphAlignment.Center);
        DrawThumbnailBorder(canvas);
    }

    private static Geometry Outline(Rectangle bounds) =>
        new RoundedRectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height,
            Math.Max(1f, Math.Min(bounds.Height, bounds.Width) * 0.50f)).Create();

    private static RectangleF DrawMarkers(Canvas canvas, Rectangle bounds, Stroke border, float scale, bool initial, bool final)
    {
        float padding = 5 * scale;
        float size = Math.Min(12 * scale, bounds.Height * 0.42f);
        float center = bounds.Top + bounds.Height / 2f;
        float left = bounds.Left + padding, right = bounds.Right - padding;
        if (initial)
        {
            float dot = size * 0.55f;
            canvas.FillEllipse(new RectangleF(left, center - dot / 2, dot, dot), border.Color);
            float tip = left + size * 1.5f;
            canvas.DrawLine(new(left + dot, center), new(tip, center), border);
            canvas.DrawLine(new(tip - size * 0.35f, center - size * 0.28f), new(tip, center), border);
            canvas.DrawLine(new(tip - size * 0.35f, center + size * 0.28f), new(tip, center), border);
            left = tip + padding;
        }
        if (final)
        {
            var ring = new RectangleF(right - size, center - size / 2, size, size);
            canvas.DrawEllipse(ring, border);
            ring.Inflate(-size * 0.23f, -size * 0.23f);
            canvas.FillEllipse(ring, border.Color);
            right -= size + padding;
        }
        return RectangleF.FromLTRB(left, bounds.Top + padding, right, bounds.Bottom - padding);
    }
}

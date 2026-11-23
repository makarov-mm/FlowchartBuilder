using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.Uml;

[GlyphFamily("UML"), GlyphName("UML class"), GlyphOrder(1)]
[GlyphDefaultWidth(64), GlyphDefaultHeight(38), GlyphMinimalWidth(24), GlyphMinimalHeight(18)]
public sealed class ClassGlyph : BlockGlyph
{
    public ClassGlyph()
    {
        Caption = "Class1";
        BackgroundColor = Color.White;
        BorderWidthInMM = 0.35f;
    }

    public override bool CanRotate => false;
    public override bool SupportsLinks => true;
    public override Type DefaultConnectorType => typeof(AssociationGlyph);

    [ActiveProperty, Category("UML"), Description("One attribute per line.")]
    [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
    public string Attributes { get; set => field = value.ReplaceLineEndings("\n"); } = string.Empty;

    [ActiveProperty, Category("UML"), Description("One method per line.")]
    [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
    public string Methods { get; set => field = value.ReplaceLineEndings("\n"); } = string.Empty;

    public override bool PreparePlacement(IWin32Window owner)
    {
        if (Core.Instance.ActiveSheet is { } sheet)
        {
            var names = sheet.Glyphs.OfType<ClassGlyph>().Select(glyph => glyph.Caption).ToHashSet(StringComparer.Ordinal);
            int number = 1;
            while (names.Contains($"Class{number}")) number++;
            Caption = $"Class{number}";
        }
        return true;
    }

    protected override void DrawGlyph(Canvas canvas)
    {
        RectangleF bounds = ScaledBounds;
        var stroke = new Stroke(BorderColor, ScaledBorderWidth);
        var font = TextStyle.FromFont(TextFont).Scale(ScaleFactor);
        float padding = 4f * ScaleFactor;
        float lineHeight = Canvas.MeasureText("Ag", font).Height;
        float header = Math.Min(bounds.Height / 3, lineHeight + 2 * padding);
        float remaining = bounds.Height - header;
        float attributeRows = 1 + Attributes.Count(character => character == '\n');
        float methodRows = 1 + Methods.Count(character => character == '\n');
        float minimum = Math.Min(remaining / 2, lineHeight + padding * 2);
        float attributeHeight = Math.Clamp(remaining * attributeRows / (attributeRows + methodRows), minimum, remaining - minimum);

        var titleBox = new RectangleF(bounds.Left, bounds.Top, bounds.Width, header);
        var attributeBox = new RectangleF(bounds.Left, titleBox.Bottom, bounds.Width, attributeHeight);
        var methodBox = RectangleF.FromLTRB(bounds.Left, attributeBox.Bottom, bounds.Right, bounds.Bottom);
        canvas.FillRectangle(bounds, BackgroundColor);
        canvas.DrawRectangle(bounds, stroke);
        canvas.DrawLine(new PointF(bounds.Left, titleBox.Bottom), new PointF(bounds.Right, titleBox.Bottom), stroke);
        canvas.DrawLine(new PointF(bounds.Left, attributeBox.Bottom), new PointF(bounds.Right, attributeBox.Bottom), stroke);

        DrawCompartment(canvas, titleBox, Caption, font with { Bold = true }, padding, isTitle: true);
        DrawCompartment(canvas, attributeBox, Attributes, font, padding);
        DrawCompartment(canvas, methodBox, Methods, font, padding);
    }

    private void DrawCompartment(Canvas canvas, RectangleF bounds, string text, TextStyle style, float padding, bool isTitle = false)
    {
        if (text.Length == 0) return;
        bounds.Inflate(-padding, -Math.Min(padding, bounds.Height / 5));
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        canvas.DrawText(text, style, bounds, ForegroundColor,
            isTitle ? TextAlignment.Center : TextAlignment.Leading,
            isTitle ? ParagraphAlignment.Center : ParagraphAlignment.Near, wrap: false);

        SizeF content = Canvas.MeasureText(text, style);
        if (content.Width <= bounds.Width && content.Height <= bounds.Height + 0.5f) return;
        SizeF marker = Canvas.MeasureText("…", style);
        var corner = RectangleF.FromLTRB(Math.Max(bounds.Left, bounds.Right - marker.Width - padding),
            Math.Max(bounds.Top, bounds.Bottom - marker.Height), bounds.Right, bounds.Bottom);
        canvas.FillRectangle(corner, BackgroundColor);
        canvas.DrawText("…", style, corner, ForegroundColor, TextAlignment.Trailing, ParagraphAlignment.Far, wrap: false);
    }

    // All three captions have already been placed in their respective compartments.
    protected override void DrawText(Canvas canvas) { }

    protected override void DrawThumbnail(Canvas canvas)
    {
        base.DrawThumbnail(canvas);
        var frame = new RectangleF(5, 8, ThumbnailWidth - 10, ThumbnailHeight - 16);
        var border = new Stroke(Settings.Colors.ThumbnailBorder, 1.25f);
        canvas.FillRectangle(frame, Color.White);
        canvas.DrawRectangle(frame, border);
        canvas.DrawLine(new(frame.Left, frame.Top + 11), new(frame.Right, frame.Top + 11), border);
        canvas.DrawLine(new(frame.Left, frame.Top + 21), new(frame.Right, frame.Top + 21), border);
        canvas.DrawText("Class", new TextStyle("Arial", 6.5f, Bold: true), new RectangleF(frame.X, frame.Y, frame.Width, 11),
            border.Color, TextAlignment.Center, ParagraphAlignment.Center, wrap: false);
        canvas.DrawLine(new(frame.Left + 4, frame.Top + 16), new(frame.Right - 12, frame.Top + 16), border);
        canvas.DrawLine(new(frame.Left + 4, frame.Top + 26), new(frame.Right - 8, frame.Top + 26), border);
        DrawThumbnailBorder(canvas);
    }
}

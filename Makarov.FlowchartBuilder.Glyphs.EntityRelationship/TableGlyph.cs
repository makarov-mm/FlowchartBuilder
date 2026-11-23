using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.EntityRelationship;

[GlyphFamily("Entity relationship"), GlyphName("Database table"), GlyphOrder(1)]
[GlyphDefaultWidth(69), GlyphDefaultHeight(36), GlyphMinimalWidth(26), GlyphMinimalHeight(16)]
public sealed class TableGlyph : BlockGlyph
{
    public TableGlyph()
    {
        Caption = "table1";
        BackgroundColor = Color.White;
        BorderWidthInMM = 0.35f;
    }

    public override bool SupportsLinks => true;
    public override bool CanRotate => false;
    public override Type DefaultConnectorType => typeof(RelationshipGlyph);

    [ActiveProperty, Category("Entity relationship"), DisplayName("Table name")]
    public new string Caption { get => base.Caption; set => base.Caption = value; }

    [ActiveProperty, Category("Entity relationship"), DisplayName("Table fields")]
    [Description("One field per line, for example id INTEGER PK or title TEXT NOT NULL.")]
    [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
    public string Fields { get; set => field = value.ReplaceLineEndings("\n"); } = "id INTEGER PK";

    [ActiveProperty, Category("Entity relationship"), DisplayName("Unique field")]
    [Description("Values in this field must be unique while the other field is NULL.")]
    public string UniqueField { get; set; } = string.Empty;

    [ActiveProperty, Category("Entity relationship"), DisplayName("While null field")]
    [Description("Apply the unique rule only to rows where this field is NULL.")]
    public string WhileNullField { get; set; } = string.Empty;

    public override bool PreparePlacement(IWin32Window owner)
    {
        var names = Core.Instance.ActiveSheet?.Glyphs.OfType<TableGlyph>().Select(table => table.Caption)
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        int number = 1;
        while (names.Contains($"table{number}")) number++;
        Caption = $"table{number}";
        return true;
    }

    protected override void DrawGlyph(Canvas canvas)
    {
        RectangleF bounds = ScaledBounds;
        var border = new Stroke(BorderColor, ScaledBorderWidth);
        var font = TextStyle.FromFont(TextFont).Scale(ScaleFactor);
        var titleFont = font with { Bold = true };
        float padding = 4f * ScaleFactor;
        float line = Canvas.MeasureText("Ag", titleFont).Height;
        float bandHeight = Math.Min(bounds.Height / 3f, line + padding * 2);
        var header = new RectangleF(bounds.X, bounds.Y, bounds.Width, bandHeight);
        var body = RectangleF.FromLTRB(bounds.Left, header.Bottom, bounds.Right, bounds.Bottom);
        canvas.FillRectangle(bounds, BackgroundColor);
        canvas.DrawRectangle(bounds, border);
        canvas.DrawLine(new(bounds.Left, header.Bottom), new(bounds.Right, header.Bottom), border);
        DrawSection(canvas, header, Caption, titleFont, padding, TextAlignment.Center, ParagraphAlignment.Center);

        if (!string.IsNullOrWhiteSpace(UniqueField) && !string.IsNullOrWhiteSpace(WhileNullField))
        {
            body.Height -= bandHeight;
            var footer = RectangleF.FromLTRB(bounds.Left, body.Bottom, bounds.Right, bounds.Bottom);
            canvas.DrawLine(new(bounds.Left, footer.Top), new(bounds.Right, footer.Top), border);
            DrawSection(canvas, footer, $"UNIQUE({UniqueField}) WHERE {WhileNullField} IS NULL",
                font with { Italic = true }, padding, TextAlignment.Leading, ParagraphAlignment.Center);
        }

        DrawSection(canvas, body, Fields, font, padding, TextAlignment.Leading, ParagraphAlignment.Near);
    }

    private void DrawSection(Canvas canvas, RectangleF section, string text, TextStyle font, float padding,
        TextAlignment alignment, ParagraphAlignment paragraph)
    {
        section.Inflate(-padding, -Math.Min(padding, section.Height / 5));
        if (section.Width <= 0 || section.Height <= 0 || text.Length == 0) return;
        canvas.DrawText(text, font, section, ForegroundColor, alignment, paragraph, wrap: false);
        var content = Canvas.MeasureText(text, font);
        if (content.Width <= section.Width && content.Height <= section.Height + 0.5f) return;
        var marker = Canvas.MeasureText("…", font);
        var corner = RectangleF.FromLTRB(Math.Max(section.Left, section.Right - marker.Width),
            Math.Max(section.Top, section.Bottom - marker.Height), section.Right, section.Bottom);
        canvas.FillRectangle(corner, BackgroundColor);
        canvas.DrawText("…", font, corner, ForegroundColor, TextAlignment.Trailing, ParagraphAlignment.Far, wrap: false);
    }

    protected override void DrawText(Canvas canvas) { }

    protected override void DrawThumbnail(Canvas canvas)
    {
        base.DrawThumbnail(canvas);
        var frame = new RectangleF(5, 8, ThumbnailWidth - 10, ThumbnailHeight - 16);
        var stroke = new Stroke(Settings.Colors.ThumbnailBorder, 1.2f);
        canvas.FillRectangle(frame, Color.White);
        canvas.DrawRectangle(frame, stroke);
        canvas.DrawLine(new(frame.Left, frame.Top + 10), new(frame.Right, frame.Top + 10), stroke);
        canvas.DrawLine(new(frame.Left + 4, frame.Top + 5), new(frame.Right - 14, frame.Top + 5), stroke);
        canvas.DrawText("PK", new TextStyle("Arial", 6f, Bold: true),
            new RectangleF(frame.Left + 3, frame.Top + 11, 12, 10), stroke.Color);
        canvas.DrawLine(new(frame.Left + 16, frame.Top + 16), new(frame.Right - 5, frame.Top + 16), stroke);
        canvas.DrawLine(new(frame.Left + 5, frame.Top + 25), new(frame.Right - 9, frame.Top + 25), stroke);
        DrawThumbnailBorder(canvas);
    }
}

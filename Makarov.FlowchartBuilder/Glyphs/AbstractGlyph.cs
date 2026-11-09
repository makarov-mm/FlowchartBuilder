using System.ComponentModel;
using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs;

/// <summary>
/// Base of everything placed on a sheet. Properties marked with <see cref="ActivePropertyAttribute"/>
/// make up the document content; drawing lives in AbstractGlyph_Draw.cs and persistence in AbstractGlyph_Serialization.cs.
/// </summary>
public abstract partial class AbstractGlyph
{
    private bool _selected;
    private bool _fixed;

    protected AbstractGlyph()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string Id { get; private set; }

    public GlyphMetadata Metadata => GlyphMetadata.Of(GetType());

    public string Name => Metadata.Name ?? Core.Instance.CurrentTranslator["UndefinedGlyphName"];

    public string Family => Metadata.Family ?? Core.Instance.CurrentTranslator["UndefinedGlyphFamily"];

    [Description("Caption.")]
    [Category("Layout")]
    [DisplayName("Caption")]
    [ActiveProperty]
    public string Caption { get; set; } = string.Empty;

    /// <exception cref="InvalidOperationException">Selecting a fixed (locked) glyph.</exception>
    public bool Selected
    {
        get => _selected;
        set
        {
            if (value && _fixed)
                throw new InvalidOperationException("A fixed glyph cannot be selected.");

            _selected = value;
        }
    }

    /// <summary>A fixed (locked) glyph ignores the mouse and cannot be selected.</summary>
    public bool Fixed
    {
        get => _fixed;
        set
        {
            _fixed = value;
            if (value)
                _selected = false;
        }
    }

    public virtual int ThumbnailWidth => 48;

    public virtual int ThumbnailHeight => 48;

    /// <summary>Palette image; the caller owns the bitmap.</summary>
    public virtual Bitmap Thumbnail
    {
        get
        {
            var bitmap = new Bitmap(ThumbnailWidth, ThumbnailHeight);

            using Graphics graphics = Graphics.FromImage(bitmap);
            using var brush = new LinearGradientBrush(
                new Point(ThumbnailWidth >> 1, 0),
                new Point(ThumbnailWidth >> 1, ThumbnailHeight),
                Settings.Colors.ThumbnailGradientStart,
                Settings.Colors.ThumbnailGradientEnd);

            graphics.Clear(Color.White);
            graphics.FillRectangle(brush, 0, 0, ThumbnailWidth, ThumbnailHeight);
            return bitmap;
        }
    }

    protected static SmoothingMode SmoothMode =>
        Settings.Environment.Antialiasing ? SmoothingMode.HighQuality : SmoothingMode.HighSpeed;

    /// <summary>Creates a glyph of the same type with a new id and a copy of all document properties.</summary>
    public AbstractGlyph Clone()
    {
        var clone = (AbstractGlyph)Activator.CreateInstance(GetType())!;

        foreach (var property in Metadata.ActiveProperties)
            property.SetValue(clone, property.GetValue(this));

        return clone;
    }

    protected void DrawThumbnailBorder(Graphics graphics) =>
        graphics.DrawRectangle(Settings.Pens.ThumbnailBorder, 0, 0, ThumbnailWidth - 1, ThumbnailHeight - 1);
}

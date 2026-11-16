using System.ComponentModel;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs;

/// <summary>
/// Base of everything placed on a sheet. Properties marked with <see cref="ActivePropertyAttribute"/>
/// make up the document content; drawing lives in AbstractGlyph_Draw.cs and persistence in AbstractGlyph_Serialization.cs.
/// </summary>
[TypeDescriptionProvider(typeof(GlyphTypeDescriptionProvider))]
public abstract partial class AbstractGlyph
{
    private bool _selected;
    private bool _fixed;

    protected AbstractGlyph()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string Id { get; private set; }

    internal void AssignNewId() => Id = Guid.NewGuid().ToString();

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

    /// <summary>Renders the palette image; the caller owns the bitmap.</summary>
    public Bitmap CreateThumbnail() => OffscreenRenderer.Render(ThumbnailWidth, ThumbnailHeight, DrawThumbnail);

    /// <summary>Creates a glyph of the same type with a new id and a copy of all document properties.</summary>
    public AbstractGlyph Clone()
    {
        var clone = (AbstractGlyph)Activator.CreateInstance(GetType())!;

        foreach (var property in Metadata.ActiveProperties)
            property.SetValue(clone, property.GetValue(this));

        return clone;
    }

    /// <summary>Whether the Properties window lists the property; glyphs hide properties that do not apply to them.</summary>
    protected internal virtual bool IsPropertyVisible(string propertyName) => true;

    /// <summary>Palette image background; glyphs draw their picture over it.</summary>
    protected virtual void DrawThumbnail(Canvas canvas) =>
        canvas.FillRectangle(
            new RectangleF(0, 0, ThumbnailWidth, ThumbnailHeight), SystemColors.Window);

    protected void DrawThumbnailBorder(Canvas canvas) =>
        canvas.DrawRectangle(new RectangleF(0, 0, ThumbnailWidth - 1, ThumbnailHeight - 1), Settings.Strokes.ThumbnailBorder);
}

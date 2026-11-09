using System.ComponentModel;

namespace Makarov.FlowchartBuilder.Glyphs;

/// <summary>Lets every glyph instance decide which of its properties the Properties window shows.</summary>
internal sealed class GlyphTypeDescriptionProvider() : TypeDescriptionProvider(TypeDescriptor.GetProvider(typeof(object)))
{
    public override ICustomTypeDescriptor? GetTypeDescriptor(Type objectType, object? instance)
    {
        ICustomTypeDescriptor? descriptor = base.GetTypeDescriptor(objectType, instance);
        return instance is AbstractGlyph glyph && descriptor is not null ? new GlyphTypeDescriptor(descriptor, glyph) : descriptor;
    }
}

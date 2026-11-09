using System.ComponentModel;

namespace Makarov.FlowchartBuilder.Glyphs;

/// <summary>Type descriptor of one glyph without the properties it reports as not applicable.</summary>
internal sealed class GlyphTypeDescriptor(ICustomTypeDescriptor parent, AbstractGlyph glyph) : CustomTypeDescriptor(parent)
{
    public override PropertyDescriptorCollection GetProperties() => Filter(base.GetProperties());

    public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes) => Filter(base.GetProperties(attributes));

    private PropertyDescriptorCollection Filter(PropertyDescriptorCollection properties) =>
        new([.. properties.Cast<PropertyDescriptor>().Where(property => glyph.IsPropertyVisible(property.Name))], readOnly: true);
}

namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>Palette group the glyph belongs to.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GlyphFamilyAttribute(string family) : GlyphAttribute
{
    public string Family { get; } = family;
}

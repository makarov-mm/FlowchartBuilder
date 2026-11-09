namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>Position of the glyph inside its palette group; glyphs without an order go last.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GlyphOrderAttribute(int order) : GlyphAttribute
{
    public int Order { get; } = order;
}

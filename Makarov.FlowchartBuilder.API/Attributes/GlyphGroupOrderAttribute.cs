namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>Position of the glyph family in the palette.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GlyphGroupOrderAttribute(int order) : GlyphAttribute
{
    public int Order { get; } = order;
}

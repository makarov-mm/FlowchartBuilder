namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>Display name of the glyph in the palette; also its default caption.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GlyphNameAttribute(string name) : GlyphAttribute
{
    public string Name { get; } = name;
}

namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>Height of a newly created glyph, in millimeters.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GlyphDefaultHeightAttribute(float heightInMM) : GlyphAttribute
{
    public float HeightInMM { get; } = heightInMM;
}

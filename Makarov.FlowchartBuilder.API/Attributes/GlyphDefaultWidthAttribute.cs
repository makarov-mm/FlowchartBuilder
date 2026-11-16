namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>Width of a newly created glyph, in millimeters.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GlyphDefaultWidthAttribute(float widthInMM) : GlyphAttribute
{
    public float WidthInMM { get; } = widthInMM;
}

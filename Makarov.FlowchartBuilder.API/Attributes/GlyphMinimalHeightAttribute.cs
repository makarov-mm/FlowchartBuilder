namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>Smallest height the glyph can be resized to, in millimeters.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GlyphMinimalHeightAttribute(float heightInMM) : GlyphAttribute
{
    public float HeightInMM { get; } = heightInMM;
}

namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>Smallest width the glyph can be resized to, in millimeters.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GlyphMinimalWidthAttribute(float widthInMM) : GlyphAttribute
{
    public float WidthInMM { get; } = widthInMM;
}

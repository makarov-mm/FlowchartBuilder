namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>Hides a glyph type from the palette when <see cref="IsVisible"/> is <see langword="false"/>.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class VisibleGlyphAttribute(bool isVisible) : GlyphAttribute
{
    public bool IsVisible { get; } = isVisible;
}

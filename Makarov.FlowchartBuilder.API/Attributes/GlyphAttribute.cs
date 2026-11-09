namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>Base class of the metadata attributes a glyph plugin puts on its glyph types.</summary>
[AttributeUsage(AttributeTargets.Class)]
public abstract class GlyphAttribute : Attribute;

namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>
/// Marks a glyph property as part of the document: it is saved to files and shown in the properties window.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ActivePropertyAttribute : Attribute;

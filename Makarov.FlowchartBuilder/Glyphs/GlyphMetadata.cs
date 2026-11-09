using System.Collections.Concurrent;
using System.Reflection;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs;

/// <summary>
/// Attributes and document properties of a glyph type, read by reflection once per type.
/// </summary>
public sealed class GlyphMetadata
{
    private static readonly ConcurrentDictionary<Type, GlyphMetadata> Cache = new();

    private readonly Dictionary<string, PropertyInfo> _activePropertiesByName;

    private GlyphMetadata(Type type)
    {
        Name = type.GetCustomAttribute<GlyphNameAttribute>()?.Name;
        Family = type.GetCustomAttribute<GlyphFamilyAttribute>()?.Family;
        Order = type.GetCustomAttribute<GlyphOrderAttribute>()?.Order;
        IsVisible = type.GetCustomAttribute<VisibleGlyphAttribute>()?.IsVisible ?? true;
        CommandName = type.GetCustomAttribute<CommandGlyphAttribute>()?.CommandName;
        DefaultWidthInMM = type.GetCustomAttribute<GlyphDefaultWidthAttribute>()?.WidthInMM ?? 0f;
        DefaultHeightInMM = type.GetCustomAttribute<GlyphDefaultHeightAttribute>()?.HeightInMM ?? 0f;
        MinimalWidthInMM = type.GetCustomAttribute<GlyphMinimalWidthAttribute>()?.WidthInMM ?? 0f;
        MinimalHeightInMM = type.GetCustomAttribute<GlyphMinimalHeightAttribute>()?.HeightInMM ?? 0f;

        ActiveProperties =
        [
            .. type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanRead && property.CanWrite && property.IsDefined(typeof(ActivePropertyAttribute))),
        ];

        _activePropertiesByName = ActiveProperties.ToDictionary(property => property.Name, StringComparer.Ordinal);
    }

    public string? Name { get; }

    public string? Family { get; }

    public int? Order { get; }

    public bool IsVisible { get; }

    /// <summary>Command run when the glyph is picked in the palette, if it is a command glyph.</summary>
    public string? CommandName { get; }

    public float DefaultWidthInMM { get; }

    public float DefaultHeightInMM { get; }

    public float MinimalWidthInMM { get; }

    public float MinimalHeightInMM { get; }

    /// <summary>Properties marked with <see cref="ActivePropertyAttribute"/>: saved to documents and shown for editing.</summary>
    public IReadOnlyList<PropertyInfo> ActiveProperties { get; }

    public static GlyphMetadata Of(Type glyphType) => Cache.GetOrAdd(glyphType, static type => new GlyphMetadata(type));

    public PropertyInfo? FindActiveProperty(string name) => _activePropertiesByName.GetValueOrDefault(name);
}

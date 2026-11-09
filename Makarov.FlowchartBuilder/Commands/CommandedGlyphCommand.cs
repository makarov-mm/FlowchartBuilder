namespace Makarov.FlowchartBuilder.Commands;

/// <summary>
/// Command run when a glyph marked with <c>CommandGlyphAttribute</c> is picked in the palette.
/// </summary>
public abstract class CommandedGlyphCommand : Command
{
    /// <summary>The glyph type that was picked.</summary>
    public Type? GlyphType { get; set; }
}

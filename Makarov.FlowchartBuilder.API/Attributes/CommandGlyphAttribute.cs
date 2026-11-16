namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>
/// Turns a palette entry into a command: selecting it runs the named command instead of arming the glyph for placement.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class CommandGlyphAttribute(string commandName) : GlyphAttribute
{
    public string CommandName { get; } = commandName;
}

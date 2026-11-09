namespace Makarov.FlowchartBuilder.API.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SheetNameAttribute(string name) : SheetAttribute
{
    public string Name { get; } = name;
}

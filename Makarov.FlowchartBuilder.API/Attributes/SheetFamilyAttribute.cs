namespace Makarov.FlowchartBuilder.API.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SheetFamilyAttribute(string family) : SheetAttribute
{
    public string Family { get; } = family;
}

namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>Paper width in millimeters.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SheetWidthAttribute(float widthInMM) : SheetAttribute
{
    public float WidthInMM { get; } = widthInMM;
}

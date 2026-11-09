namespace Makarov.FlowchartBuilder.API.Attributes;

/// <summary>Paper height in millimeters.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SheetHeightAttribute(float heightInMM) : SheetAttribute
{
    public float HeightInMM { get; } = heightInMM;
}

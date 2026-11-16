namespace Makarov.Framework.Graphics;

/// <summary>Repeating dash and gap lengths, in multiples of the stroke width.</summary>
public readonly record struct DashPattern(float Dash, float Gap);

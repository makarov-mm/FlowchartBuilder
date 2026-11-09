namespace Makarov.Framework.Graphics;

/// <summary>Two-color linear gradient between two points; outside them the end colors continue.</summary>
public readonly record struct LinearGradient(PointF Start, PointF End, Color StartColor, Color EndColor);

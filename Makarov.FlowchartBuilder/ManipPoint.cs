namespace Makarov.FlowchartBuilder;

/// <summary>
/// Resize handle of a selected block glyph.
/// </summary>
/// <param name="Coords">Handle center in sheet pixels, already rotated with the glyph.</param>
/// <param name="MoveDirection">The side or corner of the glyph the handle belongs to.</param>
/// <param name="ArrowDirection">The on-screen direction of that side after rotation; picks the mouse cursor.</param>
public readonly record struct ManipPoint(PointF Coords, MoveDirection MoveDirection, MoveDirection ArrowDirection);

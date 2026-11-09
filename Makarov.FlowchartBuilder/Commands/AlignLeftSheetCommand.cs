namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Moves the selection to the left edge of the sheet.</summary>
public sealed class AlignLeftSheetCommand : AlignOnSheetCommand
{
    protected override PointF? GetOffset(RectangleF bounds, SizeF? paper) =>
        new PointF(-bounds.Left, 0);
}

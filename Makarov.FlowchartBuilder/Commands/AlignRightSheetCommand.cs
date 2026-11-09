namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Moves the selection to the right edge of the paper.</summary>
public sealed class AlignRightSheetCommand : AlignOnSheetCommand
{
    protected override PointF? GetOffset(RectangleF bounds, SizeF? paper) =>
        paper is { } size ? new PointF(size.Width - bounds.Right, 0) : null;
}

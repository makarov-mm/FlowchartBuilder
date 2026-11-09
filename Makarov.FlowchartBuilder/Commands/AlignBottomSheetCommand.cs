namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Moves the selection to the bottom edge of the paper.</summary>
public sealed class AlignBottomSheetCommand : AlignOnSheetCommand
{
    protected override PointF? GetOffset(RectangleF bounds, SizeF? paper) =>
        paper is { } size ? new PointF(0, size.Height - bounds.Bottom) : null;
}

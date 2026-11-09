namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Centers the selection horizontally on the paper.</summary>
public sealed class AlignCenterHorizontalSheetCommand : AlignOnSheetCommand
{
    protected override PointF? GetOffset(RectangleF bounds, SizeF? paper) =>
        paper is { } size ? new PointF((size.Width - bounds.Right - bounds.Left) / 2, 0) : null;
}

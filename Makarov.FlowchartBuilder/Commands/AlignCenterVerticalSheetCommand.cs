namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Centers the selection vertically on the paper.</summary>
public sealed class AlignCenterVerticalSheetCommand : AlignOnSheetCommand
{
    protected override PointF? GetOffset(RectangleF bounds, SizeF? paper) =>
        paper is { } size ? new PointF(0, (size.Height - bounds.Bottom - bounds.Top) / 2) : null;
}

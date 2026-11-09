namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Moves the selection to the top edge of the sheet.</summary>
public sealed class AlignTopSheetCommand : AlignOnSheetCommand
{
    protected override PointF? GetOffset(RectangleF bounds, SizeF? paper) =>
        new PointF(0, -bounds.Top);
}

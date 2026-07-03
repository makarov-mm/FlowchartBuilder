using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Sheets.Iso
{
    [SheetWidth(210)]
    [SheetHeight(297)]
    [SheetName("A4")]
    public class VerticalA4 : VerticalA
    {
    }

    [SheetWidth(297)]
    [SheetHeight(210)]
    [SheetName("A4")]
    public class HorizontalA4 : HorizontalA
    {
    }
}

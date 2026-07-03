using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Sheets.Iso
{
    [SheetWidth(297)]
    [SheetHeight(420)]
    [SheetName("A3")]
    public class VerticalA3 : VerticalA
    {
    }

    [SheetWidth(420)]
    [SheetHeight(297)]
    [SheetName("A3")]
    public class HorizontalA3 : HorizontalA
    {
    }
}

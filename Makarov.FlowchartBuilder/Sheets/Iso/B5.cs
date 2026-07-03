using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Sheets.Iso
{
    [SheetWidth(176)]
    [SheetHeight(250)]
    [SheetName("B5")]
    public class VerticalB5 : VerticalB
    {
    }

    [SheetWidth(250)]
    [SheetHeight(176)]
    [SheetName("B5")]
    public class HorizontalB5 : HorizontalB
    {
    }
}

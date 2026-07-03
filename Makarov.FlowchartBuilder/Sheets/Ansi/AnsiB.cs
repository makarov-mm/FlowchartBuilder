using Makarov.FlowchartBuilder.API;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Sheets.Ansi
{
    [SheetWidth(279)]
    [SheetHeight(432)]
    [SheetName("B (Vertical layout)")]
    public class VerticalAnsiB : BaseAnsi
    {
    }

    [SheetWidth(432)]
    [SheetHeight(279)]
    [SheetName("B (Horizontal layout)")]
    public class HorizontalAnsiB : BaseAnsi
    {
    }
}

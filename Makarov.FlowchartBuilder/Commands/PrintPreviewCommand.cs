using System.ComponentModel;
using System.Drawing.Printing;

namespace Makarov.FlowchartBuilder.Commands;

public sealed class PrintPreviewCommand : Command
{
    public override void Run()
    {
        if (!Core.Instance.HasDocuments) return;
        try
        {
            using var job = DocumentPrinter.Create(Core.Instance.CurrentDocument);
            using var preview = new PrintPreviewDialog
            {
                Document = job, Width = 960, Height = 700, StartPosition = FormStartPosition.CenterParent,
                Text = Core.Instance.CurrentTranslator["Command_PrintPreview"],
            };
            preview.ShowDialog(Core.Instance.MainWindow);
        }
        catch (Exception e) when (e is InvalidPrinterException or Win32Exception or InvalidOperationException)
        {
            Core.Instance.ShowError(e.Message);
        }
    }
}

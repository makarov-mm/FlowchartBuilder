using System.ComponentModel;
using System.Drawing.Printing;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Prints the current document on one page.</summary>
public sealed class PrintCommand : Command
{
    public override void Run()
    {
        if (!Core.Instance.HasDocuments)
            return;

        try
        {
            using PrintDocument printDocument = DocumentPrinter.Create(Core.Instance.CurrentDocument);
            using var dialog = new PrintDialog
            {
                Document = printDocument,
                UseEXDialog = true,
            };

            if (dialog.ShowDialog(Core.Instance.MainWindow) == DialogResult.OK)
                printDocument.Print();
        }
        catch (Exception e) when (e is InvalidPrinterException or Win32Exception)
        {
            Core.Instance.ShowError(e.Message);
        }
    }
}

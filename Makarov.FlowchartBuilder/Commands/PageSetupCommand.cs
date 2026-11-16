using System.Drawing.Printing;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Paper, orientation and margins for printing the current document.</summary>
public sealed class PageSetupCommand : Command
{
    public override void Run()
    {
        if (!Core.Instance.HasDocuments)
            return;

        try
        {
            using var dialog = new PageSetupDialog
            {
                PageSettings = DocumentPrinter.GetPageSettings(Core.Instance.CurrentDocument),
                EnableMetric = true,
            };

            dialog.ShowDialog(Core.Instance.MainWindow);
        }
        catch (InvalidPrinterException e)
        {
            Core.Instance.ShowError(e.Message);
        }
    }
}

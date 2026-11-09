using System.Drawing.Imaging;
using Makarov.Framework.Core;

namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Saves the current sheet at 100% zoom as a PNG or BMP image.</summary>
public sealed class ExportImageCommand : Command
{
    private string _lastFileName = "Flowchart.png";

    public override void Run()
    {
        if (Core.Instance.ActiveSheet is not { } sheet)
            return;

        using var dialog = new SaveFileDialog
        {
            Filter = "PNG|*.png|BMP|*.bmp",
            FileName = _lastFileName,
        };

        if (dialog.ShowDialog(Core.Instance.MainWindow) != DialogResult.OK)
            return;

        ImageFormat? format = Path.GetExtension(dialog.FileName).ToLowerInvariant() switch
        {
            ".png" => ImageFormat.Png,
            ".bmp" => ImageFormat.Bmp,
            _ => null,
        };

        Translator translator = Core.Instance.CurrentTranslator;
        if (format is null)
        {
            MessageBox.Show(
                Core.Instance.MainWindow,
                translator["Msg_InvalidImageExtension"],
                translator["Msg_Error"],
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        try
        {
            using Bitmap image = sheet.DrawUnscaled(Core.Instance.MainWindow.InnerRectangle.Size);
            image.Save(dialog.FileName, format);
            _lastFileName = dialog.FileName;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.ExternalException)
        {
            Core.Instance.ShowError(e.Message);
        }
    }
}

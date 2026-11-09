using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Runtime.CompilerServices;
using Makarov.FlowchartBuilder.Sheets;

namespace Makarov.FlowchartBuilder;

/// <summary>
/// Prints a document on one page. The sheet is rendered with Direct2D at printer resolution (at most 300 DPI) and
/// centered in the margins: at its real size when it fits, otherwise scaled down to fit.
/// </summary>
public static class DocumentPrinter
{
    private const float MaxDpi = 300f;
    private const float ScreenDpi = 96f;
    private const double MaxPixels = 40_000_000;

    /// <summary>The printer chosen in the print dialog, kept for the session.</summary>
    private static readonly PrinterSettings Printer = new();

    private static readonly ConditionalWeakTable<Document, PageSettings> PageSettingsByDocument = [];

    /// <summary>Page settings of the document for the session; landscape for sheets wider than tall.</summary>
    public static PageSettings GetPageSettings(Document document) =>
        PageSettingsByDocument.GetValue(document, CreatePageSettings);

    /// <summary>Creates a print job for the document; the caller disposes it.</summary>
    public static PrintDocument Create(Document document)
    {
        var printDocument = new PrintDocument
        {
            DocumentName = document.DisplayName,
            PrinterSettings = Printer,
            DefaultPageSettings = GetPageSettings(document),
        };

        printDocument.PrintPage += (_, e) => PrintPage(document, e);
        return printDocument;
    }

    private static PageSettings CreatePageSettings(Document document) =>
        new(Printer) { Landscape = document.DocumentSheet is FixedSheet sheet && sheet.Width > sheet.Height };

    private static void PrintPage(Document document, PrintPageEventArgs e)
    {
        e.HasMorePages = false;
        if (e.Graphics is not { } graphics)
            return;

        Sheet sheet = document.DocumentSheet;
        Size viewport = Core.Instance.MainWindow.InnerRectangle.Size;
        Size size = sheet.GetPixelSize(scaled: false, viewport);

        // Page coordinates are hundredths of an inch; the sheet size is in screen pixels.
        RectangleF margins = e.MarginBounds;
        float width = size.Width / ScreenDpi * 100f;
        float height = size.Height / ScreenDpi * 100f;
        float fit = Math.Min(1f, Math.Min(margins.Width / width, margins.Height / height));
        var target = new RectangleF(
            margins.X + (margins.Width - width * fit) / 2f,
            margins.Y + (margins.Height - height * fit) / 2f,
            width * fit,
            height * fit);

        int printerDpi = e.PageSettings.PrinterResolution.X;
        float dpi = printerDpi > 0 ? Math.Min(MaxDpi, printerDpi) : MaxDpi;
        float resolution = dpi / ScreenDpi * fit;

        double pixels = (double)size.Width * size.Height * resolution * resolution;
        if (pixels > MaxPixels)
            resolution *= (float)Math.Sqrt(MaxPixels / pixels);

        using Bitmap image = sheet.RenderImage(viewport, resolution, forPrinting: true);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(image, target);
    }
}

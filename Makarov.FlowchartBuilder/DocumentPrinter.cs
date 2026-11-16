using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Runtime.CompilerServices;
using Makarov.FlowchartBuilder.Sheets;

namespace Makarov.FlowchartBuilder;

/// <summary>Prints the diagram at actual size on as many pages as necessary.</summary>
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
            OriginAtMargins = true,
            DocumentName = document.DisplayName,
            PrinterSettings = Printer,
            DefaultPageSettings = GetPageSettings(document),
        };

        IEnumerator<Rectangle>? pages = null;
        Rectangle content = Rectangle.Empty;
        printDocument.BeginPrint += (_, _) => { pages?.Dispose(); pages = null; };
        printDocument.EndPrint += (_, _) => { pages?.Dispose(); pages = null; };
        printDocument.PrintPage += (_, e) =>
        {
            RectangleF printable = RectangleF.Intersect(e.MarginBounds, e.PageSettings.PrintableArea);
            var available = new Size((int)Math.Floor(printable.Width), (int)Math.Floor(printable.Height));
            if (pages is null)
            {
                content = document.DocumentSheet.GetContentBounds();
                if (content.IsEmpty) return;
                pages = Tiles(content, available).GetEnumerator();
                if (!pages.MoveNext()) return;
            }
            var origin = new PointF(printable.Left - e.MarginBounds.Left, printable.Top - e.MarginBounds.Top);
            if (pages.Current == content)
            {
                origin.X += (printable.Width - content.Width * 100f / ScreenDpi) / 2f;
                origin.Y += (printable.Height - content.Height * 100f / ScreenDpi) / 2f;
            }
            PrintPage(document.DocumentSheet, pages.Current, origin, e);
            e.HasMorePages = pages.MoveNext();
        };
        return printDocument;
    }

    private static PageSettings CreatePageSettings(Document document)
    {
        Rectangle bounds = document.DocumentSheet.GetContentBounds();
        return new PageSettings(Printer) { Landscape = bounds.Width > bounds.Height };
    }

    internal static IEnumerable<Rectangle> Tiles(Rectangle content, Size margins)
    {
        int width = (int)Math.Floor(margins.Width * (double)ScreenDpi / 100);
        int height = (int)Math.Floor(margins.Height * (double)ScreenDpi / 100);
        if (width <= 0 || height <= 0)
            throw new InvalidOperationException(Core.Instance.CurrentTranslator["Msg_InvalidMargins"]);
        for (long y = content.Top; y < (long)content.Top + content.Height; y += height)
            for (long x = content.Left; x < (long)content.Left + content.Width; x += width)
                yield return new Rectangle((int)x, (int)y,
                    (int)Math.Min(width, (long)content.Left + content.Width - x),
                    (int)Math.Min(height, (long)content.Top + content.Height - y));
    }

    private static void PrintPage(Sheet sheet, Rectangle tile, PointF origin, PrintPageEventArgs e)
    {
        if (e.Graphics is not { } graphics) return;
        int printerDpi = e.PageSettings.PrinterResolution.X;
        float dpi = printerDpi > 0 ? Math.Min(MaxDpi, printerDpi) : MaxDpi;
        float resolution = Math.Min(dpi / ScreenDpi, (float)Math.Sqrt(MaxPixels / ((double)tile.Width * tile.Height)));
        using Bitmap image = sheet.RenderImage(tile.Size, resolution, forPrinting: true, region: tile);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(image, new RectangleF(origin.X, origin.Y, tile.Width * 100f / ScreenDpi, tile.Height * 100f / ScreenDpi));
    }
}

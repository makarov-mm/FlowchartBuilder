using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.Simple;

[GlyphName("Image")]
public sealed class ImageGlyph : AbstractSimpleGlyph
{
    private string _encoded = string.Empty;
    private byte[] _pixels = [];
    private Size _size;
    protected override string ThumbnailLabel => "▧";

    // Embed the original image bytes so documents, history and clipboard entries are self-contained.
    [ActiveProperty, Browsable(false)]
    public string ImageData
    {
        get => _encoded;
        set
        {
            if (value == _encoded) return;
            if (value.Length == 0) { _pixels = []; _encoded = value; _size = Size.Empty; return; }
            using var stream = new MemoryStream(Convert.FromBase64String(value));
            using var image = Image.FromStream(stream);
            // Keep the encoded original. A bounded display bitmap also fits baseline Direct2D targets.
            double scale = Math.Min(1d, 4096d / Math.Max(image.Width, image.Height));
            var size = new Size(Math.Max(1, (int)Math.Round(image.Width * scale)),
                                Math.Max(1, (int)Math.Round(image.Height * scale)));
            var bounds = new Rectangle(Point.Empty, size);
            using var bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                graphics.DrawImage(image, bounds);
            var pixels = new byte[checked(bitmap.Width * bitmap.Height * 4)];
            var data = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
            try
            {
                for (int y = 0; y < bitmap.Height; y++)
                    Marshal.Copy(data.Scan0 + y * data.Stride, pixels, y * bitmap.Width * 4, bitmap.Width * 4);
            }
            finally { bitmap.UnlockBits(data); }
            _size = size;
            _pixels = pixels;
            _encoded = value;
        }
    }

    public override bool PreparePlacement(IWin32Window owner)
    {
        using var dialog = new OpenFileDialog { Filter = Core.Instance.CurrentTranslator["ImageFiles"] + "|*.png;*.jpg;*.jpeg;*.bmp;*.gif" };
        if (dialog.ShowDialog(owner) != DialogResult.OK) return false;
        try
        {
            ImageData = Convert.ToBase64String(File.ReadAllBytes(dialog.FileName));
            HeightInMM = WidthInMM * _size.Height / _size.Width;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or
                                   OutOfMemoryException or ExternalException or InvalidDataException)
        {
            Core.Instance.ShowError(ex.Message);
            return false;
        }
    }

    protected override void DrawGlyph(Canvas canvas)
    {
        if (_pixels.Length == 0) return;
        using (canvas.Translate(3f * ScaleFactor, 3f * ScaleFactor))
            canvas.FillRectangle(ScaledBounds, Color.FromArgb(50, Color.Black));
        canvas.DrawBitmap(_pixels, _size.Width, _size.Height, ScaledBounds);
    }
}

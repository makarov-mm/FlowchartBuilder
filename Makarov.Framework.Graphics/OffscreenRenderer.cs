using System.Drawing.Imaging;
using Makarov.Framework.Graphics.Interop;

namespace Makarov.Framework.Graphics;

/// <summary>Renders with Direct2D into a memory bitmap (palette images, exported pictures, printed pages).</summary>
public static class OffscreenRenderer
{
    /// <summary>
    /// Renders <paramref name="draw"/> into a new 32-bit bitmap of the given pixel size. The canvas starts transparent,
    /// or filled with <paramref name="background"/>.
    /// </summary>
    public static Bitmap Render(int width, int height, Action<Canvas> draw, Color? background = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(draw);

        nint wicBitmap = Wic.CreateBitmap(width, height);
        nint target = 0;

        try
        {
            target = D2DFactory.CreateWicBitmapRenderTarget(
                wicBitmap,
                new Native.RenderTargetProperties
                {
                    Type = Native.D2D1_RENDER_TARGET_TYPE_DEFAULT,
                    PixelFormat = new Native.PixelFormat
                    {
                        Format = Native.DXGI_FORMAT_B8G8R8A8_UNORM,
                        AlphaMode = Native.D2D1_ALPHA_MODE_PREMULTIPLIED,
                    },
                    DpiX = 96f,
                    DpiY = 96f,
                });

            var size = new Size(width, height);
            var canvas = new Canvas(target, size, clearTypeText: false);
            int result;

            D2DRenderTarget.BeginDraw(target);
            try
            {
                canvas.BeginFrame(size);
                canvas.Clear(background ?? Color.Transparent);
                draw(canvas);
            }
            finally
            {
                result = D2DRenderTarget.EndDraw(target);
                canvas.ReleaseResources();
            }

            Com.Check(result);
            return CopyToBitmap(wicBitmap, width, height);
        }
        finally
        {
            Com.Release(ref target);
            Com.Release(ref wicBitmap);
        }
    }

    private static Bitmap CopyToBitmap(nint wicBitmap, int width, int height)
    {
        // Format32bppPArgb has the same memory layout as WIC's 32bppPBGRA.
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);

        try
        {
            Wic.CopyPixels(wicBitmap, data.Stride, data.Stride * height, data.Scan0);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        return bitmap;
    }
}

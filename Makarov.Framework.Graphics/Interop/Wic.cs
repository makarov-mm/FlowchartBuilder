namespace Makarov.Framework.Graphics.Interop;

/// <summary>The WIC imaging factory and the bitmap methods used for offscreen rendering.</summary>
internal static unsafe class Wic
{
    private static readonly Lazy<nint> Factory = new(() =>
    {
        Com.Check(Native.CoCreateInstance(
            Native.CLSID_WICImagingFactory, 0, Native.CLSCTX_INPROC_SERVER, Native.IID_IWICImagingFactory, out nint factory));
        return factory;
    });

    /// <summary>Creates a 32 bpp premultiplied BGRA bitmap, the format Direct2D renders into.</summary>
    public static nint CreateBitmap(int width, int height)
    {
        nint factory = Factory.Value;
        nint bitmap;
        Guid format = Native.GUID_WICPixelFormat32bppPBGRA;

        Com.Check(((delegate* unmanaged[Stdcall]<nint, uint, uint, Guid*, int, nint*, int>)
            Com.Method(factory, Slots.IWICImagingFactory_CreateBitmap))(
                factory, (uint)width, (uint)height, &format, Native.WICBitmapCacheOnLoad, &bitmap));

        return bitmap;
    }

    public static void CopyPixels(nint bitmap, int stride, int bufferSize, nint buffer) =>
        Com.Check(((delegate* unmanaged[Stdcall]<nint, void*, uint, uint, byte*, int>)
            Com.Method(bitmap, Slots.IWICBitmap_CopyPixels))(bitmap, null, (uint)stride, (uint)bufferSize, (byte*)buffer));
}

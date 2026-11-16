namespace Makarov.Framework.Graphics.Interop;

/// <summary>The process-wide ID2D1Factory. It is multithreaded, so its resources may be released from any thread.</summary>
internal static unsafe class D2DFactory
{
    private static readonly Lazy<nint> Factory = new(() =>
    {
        Com.Check(Native.D2D1CreateFactory(Native.D2D1_FACTORY_TYPE_MULTI_THREADED, Native.IID_ID2D1Factory, 0, out nint factory));
        return factory;
    });

    public static nint Instance => Factory.Value;

    public static nint CreatePathGeometry()
    {
        nint factory = Instance;
        nint geometry;
        Com.Check(((delegate* unmanaged[Stdcall]<nint, nint*, int>)
            Com.Method(factory, Slots.ID2D1Factory_CreatePathGeometry))(factory, &geometry));
        return geometry;
    }

    public static nint CreateStrokeStyle(Native.StrokeStyleProperties properties, ReadOnlySpan<float> dashes)
    {
        nint factory = Instance;
        nint style;

        fixed (float* dashPointer = dashes)
        {
            Com.Check(((delegate* unmanaged[Stdcall]<nint, Native.StrokeStyleProperties*, float*, uint, nint*, int>)
                Com.Method(factory, Slots.ID2D1Factory_CreateStrokeStyle))(
                    factory, &properties, dashes.IsEmpty ? null : dashPointer, (uint)dashes.Length, &style));
        }

        return style;
    }

    public static nint CreateHwndRenderTarget(Native.RenderTargetProperties properties, Native.HwndRenderTargetProperties hwndProperties)
    {
        nint factory = Instance;
        nint target;
        Com.Check(((delegate* unmanaged[Stdcall]<nint, Native.RenderTargetProperties*, Native.HwndRenderTargetProperties*, nint*, int>)
            Com.Method(factory, Slots.ID2D1Factory_CreateHwndRenderTarget))(factory, &properties, &hwndProperties, &target));
        return target;
    }

    public static nint CreateWicBitmapRenderTarget(nint wicBitmap, Native.RenderTargetProperties properties)
    {
        nint factory = Instance;
        nint target;
        Com.Check(((delegate* unmanaged[Stdcall]<nint, nint, Native.RenderTargetProperties*, nint*, int>)
            Com.Method(factory, Slots.ID2D1Factory_CreateWicBitmapRenderTarget))(factory, wicBitmap, &properties, &target));
        return target;
    }
}

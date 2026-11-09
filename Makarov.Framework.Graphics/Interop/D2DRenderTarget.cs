using System.Numerics;

namespace Makarov.Framework.Graphics.Interop;

/// <summary>Methods of ID2D1RenderTarget and ID2D1HwndRenderTarget.</summary>
internal static unsafe class D2DRenderTarget
{
    public static nint CreateSolidColorBrush(nint target, Native.ColorF color)
    {
        nint brush;
        Com.Check(((delegate* unmanaged[Stdcall]<nint, Native.ColorF*, void*, nint*, int>)
            Com.Method(target, Slots.ID2D1RenderTarget_CreateSolidColorBrush))(target, &color, null, &brush));
        return brush;
    }

    public static nint CreateGradientStopCollection(nint target, ReadOnlySpan<Native.GradientStop> stops)
    {
        nint collection;

        fixed (Native.GradientStop* stopPointer = stops)
        {
            Com.Check(((delegate* unmanaged[Stdcall]<nint, Native.GradientStop*, uint, int, int, nint*, int>)
                Com.Method(target, Slots.ID2D1RenderTarget_CreateGradientStopCollection))(
                    target, stopPointer, (uint)stops.Length, Native.D2D1_GAMMA_2_2, Native.D2D1_EXTEND_MODE_CLAMP, &collection));
        }

        return collection;
    }

    public static nint CreateLinearGradientBrush(nint target, Native.LinearGradientBrushProperties properties, nint stops)
    {
        nint brush;
        Com.Check(((delegate* unmanaged[Stdcall]<nint, Native.LinearGradientBrushProperties*, void*, nint, nint*, int>)
            Com.Method(target, Slots.ID2D1RenderTarget_CreateLinearGradientBrush))(target, &properties, null, stops, &brush));
        return brush;
    }

    public static void DrawLine(nint target, Native.Point2F point0, Native.Point2F point1, nint brush, float width, nint style) =>
        ((delegate* unmanaged[Stdcall]<nint, Native.Point2F, Native.Point2F, nint, float, nint, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_DrawLine))(target, point0, point1, brush, width, style);

    public static void DrawRectangle(nint target, Native.RectF rect, nint brush, float width, nint style) =>
        ((delegate* unmanaged[Stdcall]<nint, Native.RectF*, nint, float, nint, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_DrawRectangle))(target, &rect, brush, width, style);

    public static void FillRectangle(nint target, Native.RectF rect, nint brush) =>
        ((delegate* unmanaged[Stdcall]<nint, Native.RectF*, nint, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_FillRectangle))(target, &rect, brush);

    public static void DrawEllipse(nint target, Native.Ellipse ellipse, nint brush, float width, nint style) =>
        ((delegate* unmanaged[Stdcall]<nint, Native.Ellipse*, nint, float, nint, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_DrawEllipse))(target, &ellipse, brush, width, style);

    public static void FillEllipse(nint target, Native.Ellipse ellipse, nint brush) =>
        ((delegate* unmanaged[Stdcall]<nint, Native.Ellipse*, nint, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_FillEllipse))(target, &ellipse, brush);

    public static void DrawGeometry(nint target, nint geometry, nint brush, float width, nint style) =>
        ((delegate* unmanaged[Stdcall]<nint, nint, nint, float, nint, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_DrawGeometry))(target, geometry, brush, width, style);

    public static void FillGeometry(nint target, nint geometry, nint brush) =>
        ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_FillGeometry))(target, geometry, brush, 0);

    public static void DrawTextLayout(nint target, Native.Point2F origin, nint layout, nint brush, int options) =>
        ((delegate* unmanaged[Stdcall]<nint, Native.Point2F, nint, nint, int, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_DrawTextLayout))(target, origin, layout, brush, options);

    public static void SetTransform(nint target, Matrix3x2 transform) =>
        ((delegate* unmanaged[Stdcall]<nint, Matrix3x2*, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_SetTransform))(target, &transform);

    public static void SetAntialiasMode(nint target, int mode) =>
        ((delegate* unmanaged[Stdcall]<nint, int, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_SetAntialiasMode))(target, mode);

    public static void SetTextAntialiasMode(nint target, int mode) =>
        ((delegate* unmanaged[Stdcall]<nint, int, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_SetTextAntialiasMode))(target, mode);

    public static void PushAxisAlignedClip(nint target, Native.RectF rect, int antialiasMode) =>
        ((delegate* unmanaged[Stdcall]<nint, Native.RectF*, int, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_PushAxisAlignedClip))(target, &rect, antialiasMode);

    public static void PopAxisAlignedClip(nint target) =>
        ((delegate* unmanaged[Stdcall]<nint, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_PopAxisAlignedClip))(target);

    public static void Clear(nint target, Native.ColorF color) =>
        ((delegate* unmanaged[Stdcall]<nint, Native.ColorF*, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_Clear))(target, &color);

    public static void BeginDraw(nint target) =>
        ((delegate* unmanaged[Stdcall]<nint, void>)
            Com.Method(target, Slots.ID2D1RenderTarget_BeginDraw))(target);

    /// <returns>The HRESULT; <see cref="Native.D2DERR_RECREATE_TARGET"/> means the device was lost.</returns>
    public static int EndDraw(nint target) =>
        ((delegate* unmanaged[Stdcall]<nint, ulong*, ulong*, int>)
            Com.Method(target, Slots.ID2D1RenderTarget_EndDraw))(target, null, null);

    /// <summary>ID2D1HwndRenderTarget::Resize.</summary>
    public static void Resize(nint hwndTarget, uint width, uint height)
    {
        var size = new Native.SizeU(width, height);
        Com.Check(((delegate* unmanaged[Stdcall]<nint, Native.SizeU*, int>)
            Com.Method(hwndTarget, Slots.ID2D1HwndRenderTarget_Resize))(hwndTarget, &size));
    }
}

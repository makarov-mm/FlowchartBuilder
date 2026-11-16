namespace Makarov.Framework.Graphics.Interop;

/// <summary>
/// Vtable slots of the COM methods used by the renderer, counted from the start of the vtable
/// (IUnknown occupies slots 0-2). Names are "Interface_Method", as declared in d2d1.h, dwrite.h and wincodec.h.
/// </summary>
internal static class Slots
{
    public const int IUnknown_Release = 2;

    public const int ID2D1Factory_CreatePathGeometry = 10;
    public const int ID2D1Factory_CreateStrokeStyle = 11;
    public const int ID2D1Factory_CreateWicBitmapRenderTarget = 13;
    public const int ID2D1Factory_CreateHwndRenderTarget = 14;

    public const int ID2D1RenderTarget_CreateBitmap = 4;
    public const int ID2D1RenderTarget_DrawBitmap = 26;
    public const int ID2D1RenderTarget_CreateSolidColorBrush = 8;
    public const int ID2D1RenderTarget_CreateGradientStopCollection = 9;
    public const int ID2D1RenderTarget_CreateLinearGradientBrush = 10;
    public const int ID2D1RenderTarget_DrawLine = 15;
    public const int ID2D1RenderTarget_DrawRectangle = 16;
    public const int ID2D1RenderTarget_FillRectangle = 17;
    public const int ID2D1RenderTarget_DrawEllipse = 20;
    public const int ID2D1RenderTarget_FillEllipse = 21;
    public const int ID2D1RenderTarget_DrawGeometry = 22;
    public const int ID2D1RenderTarget_FillGeometry = 23;
    public const int ID2D1RenderTarget_DrawTextLayout = 28;
    public const int ID2D1RenderTarget_SetTransform = 30;
    public const int ID2D1RenderTarget_SetAntialiasMode = 32;
    public const int ID2D1RenderTarget_SetTextAntialiasMode = 34;
    public const int ID2D1RenderTarget_PushAxisAlignedClip = 45;
    public const int ID2D1RenderTarget_PopAxisAlignedClip = 46;
    public const int ID2D1RenderTarget_Clear = 47;
    public const int ID2D1RenderTarget_BeginDraw = 48;
    public const int ID2D1RenderTarget_EndDraw = 49;

    public const int ID2D1HwndRenderTarget_Resize = 58;

    public const int ID2D1PathGeometry_Open = 17;

    public const int ID2D1GeometrySink_SetFillMode = 3;
    public const int ID2D1GeometrySink_BeginFigure = 5;
    public const int ID2D1GeometrySink_EndFigure = 8;
    public const int ID2D1GeometrySink_Close = 9;
    public const int ID2D1GeometrySink_AddLine = 10;
    public const int ID2D1GeometrySink_AddBezier = 11;
    public const int ID2D1GeometrySink_AddArc = 14;

    public const int IDWriteFactory_CreateTextFormat = 15;
    public const int IDWriteFactory_CreateTextLayout = 18;

    public const int IDWriteTextLayout_SetTextAlignment = 3;
    public const int IDWriteTextLayout_SetParagraphAlignment = 4;
    public const int IDWriteTextLayout_SetWordWrapping = 5;
    public const int IDWriteTextLayout_SetUnderline = 36;
    public const int IDWriteTextLayout_SetStrikethrough = 37;
    public const int IDWriteTextLayout_GetMetrics = 60;

    public const int IWICImagingFactory_CreateBitmap = 17;

    public const int IWICBitmap_CopyPixels = 7;
}

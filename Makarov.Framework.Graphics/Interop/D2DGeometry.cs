namespace Makarov.Framework.Graphics.Interop;

/// <summary>Methods of ID2D1PathGeometry and ID2D1GeometrySink.</summary>
internal static unsafe class D2DGeometry
{
    public static nint Open(nint pathGeometry)
    {
        nint sink;
        Com.Check(((delegate* unmanaged[Stdcall]<nint, nint*, int>)
            Com.Method(pathGeometry, Slots.ID2D1PathGeometry_Open))(pathGeometry, &sink));
        return sink;
    }

    public static void SetFillMode(nint sink, int fillMode) =>
        ((delegate* unmanaged[Stdcall]<nint, int, void>)
            Com.Method(sink, Slots.ID2D1GeometrySink_SetFillMode))(sink, fillMode);

    public static void BeginFigure(nint sink, Native.Point2F start, int figureBegin) =>
        ((delegate* unmanaged[Stdcall]<nint, Native.Point2F, int, void>)
            Com.Method(sink, Slots.ID2D1GeometrySink_BeginFigure))(sink, start, figureBegin);

    public static void EndFigure(nint sink, int figureEnd) =>
        ((delegate* unmanaged[Stdcall]<nint, int, void>)
            Com.Method(sink, Slots.ID2D1GeometrySink_EndFigure))(sink, figureEnd);

    public static void AddLine(nint sink, Native.Point2F point) =>
        ((delegate* unmanaged[Stdcall]<nint, Native.Point2F, void>)
            Com.Method(sink, Slots.ID2D1GeometrySink_AddLine))(sink, point);

    public static void AddBezier(nint sink, Native.BezierSegment bezier) =>
        ((delegate* unmanaged[Stdcall]<nint, Native.BezierSegment*, void>)
            Com.Method(sink, Slots.ID2D1GeometrySink_AddBezier))(sink, &bezier);

    public static void AddArc(nint sink, Native.ArcSegment arc) =>
        ((delegate* unmanaged[Stdcall]<nint, Native.ArcSegment*, void>)
            Com.Method(sink, Slots.ID2D1GeometrySink_AddArc))(sink, &arc);

    public static void Close(nint sink) =>
        Com.Check(((delegate* unmanaged[Stdcall]<nint, int>)
            Com.Method(sink, Slots.ID2D1GeometrySink_Close))(sink));
}

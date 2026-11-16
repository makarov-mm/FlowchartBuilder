using System.Numerics;
using Makarov.Framework.Graphics.Interop;

namespace Makarov.Framework.Graphics;

/// <summary>
/// Hardware-accelerated drawing surface over a Direct2D render target, with a small GDI+-like API. Coordinates
/// are pixels (the targets use 96 DPI). A canvas is handed out by <see cref="CanvasControl"/> and
/// <see cref="OffscreenRenderer"/> and is valid only while drawing.
/// </summary>
public sealed class Canvas
{
    private readonly nint _target;
    private readonly int _smoothTextMode;
    private readonly Dictionary<int, nint> _brushes = [];
    private readonly Dictionary<(int, int), nint> _gradientStops = [];
    private Matrix3x2 _transform = Matrix3x2.Identity;
    private bool _antialiasing = true;

    internal Canvas(nint target, Size size, bool clearTypeText)
    {
        _target = target;
        Size = size;
        _smoothTextMode = clearTypeText ? Native.D2D1_TEXT_ANTIALIAS_MODE_CLEARTYPE : Native.D2D1_TEXT_ANTIALIAS_MODE_GRAYSCALE;
    }

    /// <summary>Size of the drawing surface in pixels.</summary>
    public Size Size { get; private set; }

    /// <summary>Smooth (antialiased) shapes and text; off gives crisp, aliased output.</summary>
    public bool Antialiasing
    {
        get => _antialiasing;
        set
        {
            _antialiasing = value;
            ApplyAntialiasing();
        }
    }

    /// <summary>Current transform; drawing coordinates are mapped through it.</summary>
    public Matrix3x2 Transform
    {
        get => _transform;
        set
        {
            _transform = value;
            D2DRenderTarget.SetTransform(_target, value);
        }
    }

    /// <summary>Applies <paramref name="transform"/> before the current transform until the scope is disposed.</summary>
    public CanvasScope PushTransform(Matrix3x2 transform)
    {
        var scope = new CanvasScope(this, _transform, clip: false);
        Transform = transform * _transform;
        return scope;
    }

    public CanvasScope Translate(float dx, float dy) => PushTransform(Matrix3x2.CreateTranslation(dx, dy));

    /// <summary>Rotates clockwise by <paramref name="degrees"/> around <paramref name="center"/>.</summary>
    public CanvasScope Rotate(float degrees, PointF center) =>
        PushTransform(Matrix3x2.CreateRotation(float.DegreesToRadians(degrees), new Vector2(center.X, center.Y)));

    public CanvasScope Scale(float factor) => PushTransform(Matrix3x2.CreateScale(factor));

    /// <summary>Restricts drawing to <paramref name="rect"/> (in current coordinates) until the scope is disposed.</summary>
    public CanvasScope Clip(RectangleF rect)
    {
        D2DRenderTarget.PushAxisAlignedClip(_target, ToRect(rect), Native.D2D1_ANTIALIAS_MODE_ALIASED);
        return new CanvasScope(this, _transform, clip: true);
    }

    public static SizeF MeasureText(string text, TextStyle style, float maxWidth = TextLayouts.Unbounded) =>
        TextLayouts.Measure(text, style, maxWidth);

    public void Clear(Color color) => D2DRenderTarget.Clear(_target, ToColor(color));

    public void FillRectangle(RectangleF rect, Color color)
    {
        if (color.A != 0)
            D2DRenderTarget.FillRectangle(_target, ToRect(rect), GetBrush(color));
    }

    public void FillRectangle(RectangleF rect, LinearGradient gradient)
    {
        nint brush = CreateGradientBrush(gradient);
        try
        {
            D2DRenderTarget.FillRectangle(_target, ToRect(rect), brush);
        }
        finally
        {
            Com.Release(ref brush);
        }
    }

    /// <summary>Strokes the rectangle; with an odd pixel width and integer coordinates it covers whole pixels, like GDI+.</summary>
    public void DrawRectangle(RectangleF rect, Stroke stroke)
    {
        if (stroke.Color.A == 0 || stroke.Width <= 0f)
            return;

        float offset = IsIntegral(rect.X) && IsIntegral(rect.Y) ? SnapOffset(stroke.Width) : 0f;
        rect.Offset(offset, offset);
        D2DRenderTarget.DrawRectangle(_target, ToRect(rect), GetBrush(stroke.Color), stroke.Width, StrokeStyles.Get(stroke));
    }

    /// <summary>Draws a line; axis-aligned lines on integer coordinates are kept crisp.</summary>
    public void DrawLine(PointF from, PointF to, Stroke stroke)
    {
        if (stroke.Color.A == 0 || stroke.Width <= 0f)
            return;

        float offset = SnapOffset(stroke.Width);
        if (offset != 0f && from.X == to.X && IsIntegral(from.X))
        {
            from.X += offset;
            to.X += offset;
        }
        else if (offset != 0f && from.Y == to.Y && IsIntegral(from.Y))
        {
            from.Y += offset;
            to.Y += offset;
        }

        D2DRenderTarget.DrawLine(_target, ToPoint(from), ToPoint(to), GetBrush(stroke.Color), stroke.Width, StrokeStyles.Get(stroke));
    }

    public void FillEllipse(RectangleF bounds, Color color)
    {
        if (color.A != 0)
            D2DRenderTarget.FillEllipse(_target, ToEllipse(bounds), GetBrush(color));
    }

    public void DrawEllipse(RectangleF bounds, Stroke stroke)
    {
        if (stroke.Color.A != 0 && stroke.Width > 0f)
            D2DRenderTarget.DrawEllipse(_target, ToEllipse(bounds), GetBrush(stroke.Color), stroke.Width, StrokeStyles.Get(stroke));
    }

    public void FillGeometry(Geometry geometry, Color color)
    {
        if (color.A != 0)
            D2DRenderTarget.FillGeometry(_target, geometry.GetNative(), GetBrush(color));
    }

    public void FillGeometry(Geometry geometry, LinearGradient gradient)
    {
        nint brush = CreateGradientBrush(gradient);
        try
        {
            D2DRenderTarget.FillGeometry(_target, geometry.GetNative(), brush);
        }
        finally
        {
            Com.Release(ref brush);
        }
    }

    public void DrawGeometry(Geometry geometry, Stroke stroke)
    {
        if (stroke.Color.A != 0 && stroke.Width > 0f)
            D2DRenderTarget.DrawGeometry(_target, geometry.GetNative(), GetBrush(stroke.Color), stroke.Width, StrokeStyles.Get(stroke));
    }

    /// <summary>Draws text inside <paramref name="layout"/>, clipped to it.</summary>
    public void DrawText(
        string text,
        TextStyle style,
        RectangleF layout,
        Color color,
        TextAlignment alignment = TextAlignment.Leading,
        ParagraphAlignment paragraph = ParagraphAlignment.Near,
        bool wrap = true)
    {
        if (string.IsNullOrEmpty(text) || layout.Width <= 0f || layout.Height <= 0f || color.A == 0)
            return;

        nint textLayout = TextLayouts.Create(text, style, layout.Width, layout.Height, alignment, paragraph, wrap);
        try
        {
            D2DRenderTarget.DrawTextLayout(
                _target, ToPoint(layout.Location), textLayout, GetBrush(color), Native.D2D1_DRAW_TEXT_OPTIONS_CLIP);
        }
        finally
        {
            Com.Release(ref textLayout);
        }
    }

    /// <summary>Draws one line of text with its top-left corner at <paramref name="location"/>.</summary>
    public void DrawText(string text, TextStyle style, PointF location, Color color)
    {
        if (string.IsNullOrEmpty(text) || color.A == 0)
            return;

        nint textLayout = TextLayouts.Create(
            text, style, TextLayouts.Unbounded, TextLayouts.Unbounded, TextAlignment.Leading, ParagraphAlignment.Near, wrap: false);
        try
        {
            D2DRenderTarget.DrawTextLayout(_target, ToPoint(location), textLayout, GetBrush(color), 0);
        }
        finally
        {
            Com.Release(ref textLayout);
        }
    }

    /// <summary>Resets the per-frame state; called after BeginDraw.</summary>
    internal void BeginFrame(Size size)
    {
        Size = size;
        _antialiasing = true;
        Transform = Matrix3x2.Identity;
        ApplyAntialiasing();
    }

    internal void Restore(Matrix3x2 transform, bool popClip)
    {
        if (popClip)
            D2DRenderTarget.PopAxisAlignedClip(_target);

        Transform = transform;
    }

    /// <summary>Releases the brushes created for the render target.</summary>
    internal void ReleaseResources()
    {
        foreach (nint brush in _brushes.Values)
        {
            nint value = brush;
            Com.Release(ref value);
        }

        foreach (nint stops in _gradientStops.Values)
        {
            nint value = stops;
            Com.Release(ref value);
        }

        _brushes.Clear();
        _gradientStops.Clear();
    }

    private static bool IsIntegral(float value) => value == MathF.Round(value);

    private static Native.ColorF ToColor(Color color) => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    private static Native.Point2F ToPoint(PointF point) => new(point.X, point.Y);

    private static Native.RectF ToRect(RectangleF rect) => new(rect.Left, rect.Top, rect.Right, rect.Bottom);

    private static Native.Ellipse ToEllipse(RectangleF bounds) =>
        new(new Native.Point2F(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f), bounds.Width / 2f, bounds.Height / 2f);

    /// <summary>Half a pixel for odd stroke widths under a whole-pixel translation, so that hairlines stay sharp.</summary>
    private float SnapOffset(float width)
    {
        bool translationOnly = _transform.M11 == 1f && _transform.M22 == 1f && _transform.M12 == 0f && _transform.M21 == 0f;
        bool oddWidth = width is 1f or 3f;

        return translationOnly && oddWidth && IsIntegral(_transform.M31) && IsIntegral(_transform.M32) ? 0.5f : 0f;
    }

    private void ApplyAntialiasing()
    {
        D2DRenderTarget.SetAntialiasMode(
            _target, _antialiasing ? Native.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE : Native.D2D1_ANTIALIAS_MODE_ALIASED);
        D2DRenderTarget.SetTextAntialiasMode(
            _target, _antialiasing ? _smoothTextMode : Native.D2D1_TEXT_ANTIALIAS_MODE_ALIASED);
    }

    private nint GetBrush(Color color)
    {
        int key = color.ToArgb();
        if (!_brushes.TryGetValue(key, out nint brush))
        {
            brush = D2DRenderTarget.CreateSolidColorBrush(_target, ToColor(color));
            _brushes.Add(key, brush);
        }

        return brush;
    }

    private nint CreateGradientBrush(LinearGradient gradient)
    {
        var key = (gradient.StartColor.ToArgb(), gradient.EndColor.ToArgb());
        if (!_gradientStops.TryGetValue(key, out nint stops))
        {
            stops = D2DRenderTarget.CreateGradientStopCollection(
                _target,
                [
                    new Native.GradientStop(0f, ToColor(gradient.StartColor)),
                    new Native.GradientStop(1f, ToColor(gradient.EndColor)),
                ]);
            _gradientStops.Add(key, stops);
        }

        return D2DRenderTarget.CreateLinearGradientBrush(
            _target, new Native.LinearGradientBrushProperties(ToPoint(gradient.Start), ToPoint(gradient.End)), stops);
    }
}

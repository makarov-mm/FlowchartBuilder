using System.ComponentModel;
using Makarov.Framework.Graphics.Interop;

namespace Makarov.Framework.Graphics;

/// <summary>
/// Control painted with Direct2D through an ID2D1HwndRenderTarget. Handle <see cref="Render"/> to draw;
/// call <see cref="Control.Invalidate()"/> to repaint.
/// </summary>
public class CanvasControl : Control
{
    private nint _target;
    private Canvas? _canvas;

    public CanvasControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw,
            true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
    }

    [Category("Appearance")]
    public event EventHandler<CanvasRenderEventArgs>? Render;

    protected virtual void OnRender(CanvasRenderEventArgs e) => Render?.Invoke(this, e);

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Direct2D paints every pixel of the control.
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (DesignMode)
        {
            // The form designer gets a plain area instead of a Direct2D target.
            e.Graphics.Clear(BackColor);
            return;
        }

        Size size = ClientSize;
        if (size.Width <= 0 || size.Height <= 0)
            return;

        Canvas canvas = EnsureCanvas(size);
        int result;

        D2DRenderTarget.BeginDraw(_target);
        try
        {
            canvas.BeginFrame(size);
            OnRender(new CanvasRenderEventArgs(canvas));
        }
        finally
        {
            result = D2DRenderTarget.EndDraw(_target);
        }

        if (result == Native.D2DERR_RECREATE_TARGET)
        {
            // The graphics device was lost (driver update, remote session, ...): start over with a new target.
            ReleaseTarget();
            Invalidate();
            return;
        }

        Com.Check(result);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        Size size = ClientSize;
        if (_target != 0 && size.Width > 0 && size.Height > 0)
            D2DRenderTarget.Resize(_target, (uint)size.Width, (uint)size.Height);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        ReleaseTarget();
        base.OnHandleDestroyed(e);
    }

    protected override void Dispose(bool disposing)
    {
        ReleaseTarget();
        base.Dispose(disposing);
    }

    private Canvas EnsureCanvas(Size size)
    {
        if (_canvas is not null)
            return _canvas;

        _target = D2DFactory.CreateHwndRenderTarget(
            new Native.RenderTargetProperties
            {
                Type = Native.D2D1_RENDER_TARGET_TYPE_DEFAULT,
                PixelFormat = new Native.PixelFormat
                {
                    Format = Native.DXGI_FORMAT_B8G8R8A8_UNORM,
                    AlphaMode = Native.D2D1_ALPHA_MODE_IGNORE,
                },
                DpiX = 96f,
                DpiY = 96f,
            },
            new Native.HwndRenderTargetProperties
            {
                Hwnd = Handle,
                PixelWidth = (uint)size.Width,
                PixelHeight = (uint)size.Height,
                PresentOptions = Native.D2D1_PRESENT_OPTIONS_IMMEDIATELY,
            });

        _canvas = new Canvas(_target, size, clearTypeText: true);
        return _canvas;
    }

    private void ReleaseTarget()
    {
        _canvas?.ReleaseResources();
        _canvas = null;
        Com.Release(ref _target);
    }
}

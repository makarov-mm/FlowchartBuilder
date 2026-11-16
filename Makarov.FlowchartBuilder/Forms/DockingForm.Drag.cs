namespace Makarov.FlowchartBuilder.Forms;

public partial class DockingForm : IMessageFilter
{
    private Point? _press;
    private Point _grabOffset;
    private DockPreview? _preview;

    private void StartDrag(Point screen)
    {
        if (_editor is null || _press.HasValue) return;
        _press = screen;
        Rectangle current = TopLevel ? Bounds : _editor.RectangleToScreen(Bounds);
        _grabOffset = new Point(Math.Clamp(screen.X - current.Left, 0, _floating.Width - 1),
                                Math.Clamp(screen.Y - current.Top, 0, LogicalToDeviceUnits(24)));
        Application.AddMessageFilter(this);
        Capture = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_press is not Point start || _editor is null) return;
        Point screen = PointToScreen(e.Location);
        Size threshold = SystemInformation.DragSize;
        if (_preview is null && new Rectangle(start.X - threshold.Width / 2, start.Y - threshold.Height / 2,
                                               threshold.Width, threshold.Height).Contains(screen))
            return;

        DockStyle side = _editor.DropSide(screen);
        _preview ??= new DockPreview();
        _preview.Bounds = side == DockStyle.None ? FloatingAt(screen) : _editor.ArrangeTools(this, side);
        _preview.Opacity = side == DockStyle.None ? 0.15 : 0.35;
        if (!_preview.Visible) _preview.Show(TopLevel ? this : _editor);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left || !_press.HasValue || _editor is null) return;
        bool moved = _preview is not null;
        Point screen = PointToScreen(e.Location);
        CancelDrag();
        if (moved) Place(_editor.DropSide(screen), FloatingAt(screen));
    }

    private Rectangle FloatingAt(Point screen) =>
        new(new Point(screen.X - _grabOffset.X, screen.Y - _grabOffset.Y), _floating.Size);

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);
        if (!Capture && _press.HasValue) CancelDrag();
    }

    private void CancelDrag()
    {
        if (!_press.HasValue) return;
        _press = null;
        Application.RemoveMessageFilter(this);
        if (Capture) Capture = false;
        _preview?.Dispose();
        _preview = null;
    }

    bool IMessageFilter.PreFilterMessage(ref Message m)
    {
        const int WM_KEYDOWN = 0x0100;
        if (m.Msg != WM_KEYDOWN || (Keys)m.WParam != Keys.Escape) return false;
        CancelDrag();
        return true;
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_NCLBUTTONDOWN = 0x00A1, WM_NCLBUTTONDBLCLK = 0x00A3, HTCAPTION = 2;
        if (_editor is not null && TopLevel && m.WParam == HTCAPTION)
        {
            if (m.Msg == WM_NCLBUTTONDOWN)
            {
                Activate();
                int packed = unchecked((int)m.LParam);
                StartDrag(new Point((short)packed, (short)(packed >> 16)));
                return;
            }
            if (m.Msg == WM_NCLBUTTONDBLCLK)
            {
                CancelDrag();
                Place(_lastSide, _floating);
                return;
            }
        }
        base.WndProc(ref m);
    }

    private sealed class DockPreview : Form
    {
        public DockPreview()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            BackColor = SystemColors.Highlight;
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_NOACTIVATE = 0x08000000, WS_EX_TRANSPARENT = 0x20;
                CreateParams parameters = base.CreateParams;
                parameters.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TRANSPARENT;
                return parameters;
            }
        }
    }
}

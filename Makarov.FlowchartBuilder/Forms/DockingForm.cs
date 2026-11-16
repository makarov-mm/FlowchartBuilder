using System.Globalization;
using Makarov.FlowchartBuilder.Commands;

namespace Makarov.FlowchartBuilder.Forms;

/// <summary>A tool can be a child of the editor or an independently resizable, owned window.</summary>
public partial class DockingForm : Form
{
    private readonly Panel _header = new() { Dock = DockStyle.Top, BackColor = SystemColors.ControlLight };
    private readonly Label _caption = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, Cursor = Cursors.SizeAll };
    private readonly Button _close = new() { Dock = DockStyle.Right, Text = "×", TabStop = false, FlatStyle = FlatStyle.Flat };
    private readonly Panel _grip = new() { BackColor = SystemColors.ControlDark };
    private MainForm? _editor;
    private Command? _showCommand;
    private DockStyle _defaultSide;
    private DockStyle _lastSide;
    private Rectangle _floating;
    private Point? _resizeStart;
    private int _initialExtent;

    internal DockStyle Side { get; private set; }
    internal bool IsOpen { get; private set; }
    internal int DockWidth { get; private set; }
    internal int DockHeight { get; private set; }
    private string Key(string name) => $"{Settings.Keys.SecondaryWindowParams}.{GetType().Name}.{name}";

    public DockingForm()
    {
        InitializeComponent();
        _header.Height = LogicalToDeviceUnits(25);
        _caption.Padding = new Padding(LogicalToDeviceUnits(6), 0, 0, 0);
        _close.Width = _header.Height;
        _close.FlatAppearance.BorderSize = 0;
        _close.Click += (_, _) => HideTool();
        _header.Controls.Add(_caption);
        _header.Controls.Add(_close);
        Controls.Add(_header);
        Controls.Add(_grip);
        _header.Hide();
        _grip.Hide();
        TextChanged += (_, _) => _caption.Text = Text;
        _caption.MouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Left) return;
            Point screen = _caption.PointToScreen(e.Location);
            if (e.Clicks == 2)
            {
                CancelDrag();
                Place(DockStyle.None, new Rectangle(screen.X - e.X, screen.Y - e.Y, _floating.Width, _floating.Height));
            }
            else StartDrag(screen);
        };
        _grip.MouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Left) return;
            _resizeStart = _grip.PointToScreen(e.Location);
            _initialExtent = Side is DockStyle.Left or DockStyle.Right ? Width : Height;
            _grip.Capture = true;
        };
        _grip.MouseMove += ResizeDock;
        _grip.MouseUp += (_, _) => { _resizeStart = null; _grip.Capture = false; };
        _grip.MouseCaptureChanged += (_, _) => { if (!_grip.Capture) _resizeStart = null; };
        Disposed += (_, _) => CancelDrag();
    }

    internal void RestorePlacement(MainForm editor, DockStyle defaultSide, Command showCommand)
    {
        _editor = editor;
        _defaultSide = defaultSide;
        _showCommand = showCommand;
        _close.AccessibleName = Core.Instance.CurrentTranslator["Close"];
        var settings = Core.Instance.SettingsStorage;
        int Int(string name, int fallback) =>
            int.TryParse(settings[Key(name)], NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : fallback;
        bool Flag(string name) => bool.TryParse(settings[Key(name)], out bool b) && b;
        DockStyle ReadSide(string name, DockStyle fallback) =>
            Enum.TryParse(settings[Key(name)], out DockStyle side) &&
            side is DockStyle.None or DockStyle.Left or DockStyle.Right or DockStyle.Top or DockStyle.Bottom ? side : fallback;

        // Old settings stored four snap flags and floating bounds in screen coordinates.
        DockStyle legacy = Flag("DockLeft") ? DockStyle.Left : Flag("DockRight") ? DockStyle.Right :
            Flag("DockTop") ? DockStyle.Top : Flag("DockBottom") ? DockStyle.Bottom :
            settings.Contains(Key("Left")) ? DockStyle.None : defaultSide;
        Side = ReadSide("DockSide", legacy);
        _lastSide = ReadSide("LastDockSide", defaultSide);
        if (_lastSide == DockStyle.None) _lastSide = defaultSide;
        Point start = editor.PointToScreen(new Point(40, 80));
        int minimum = LogicalToDeviceUnits(100);
        _floating = FitToScreen(new Rectangle(Int("Left", start.X), Int("Top", start.Y),
            Math.Max(minimum, Int("Width", Width)), Math.Max(minimum, Int("Height", Height))));
        DockWidth = Math.Max(minimum, Int("DockWidth", Int("Width", DefaultWidthForSide())));
        DockHeight = Math.Max(minimum, Int("DockHeight", LogicalToDeviceUnits(200)));
        if (!bool.TryParse(settings[Key("Visible")], out bool visible) || visible) ShowTool();
        else _showCommand.Enabled = true;
    }

    private int DefaultWidthForSide() => LogicalToDeviceUnits(_defaultSide == DockStyle.Left ? 234 : 310);

    internal void ResetPlacement()
    {
        DockWidth = DefaultWidthForSide();
        DockHeight = LogicalToDeviceUnits(200);
        Place(_defaultSide, _floating);
    }

    internal void SavePlacement()
    {
        if (Side == DockStyle.None && IsOpen) _floating = Bounds;
        var settings = Core.Instance.SettingsStorage;
        settings[Key("DockSide")] = Side.ToString();
        settings[Key("LastDockSide")] = _lastSide.ToString();
        settings[Key("Visible")] = IsOpen.ToString();
        settings[Key("Left")] = _floating.Left.ToString(CultureInfo.InvariantCulture);
        settings[Key("Top")] = _floating.Top.ToString(CultureInfo.InvariantCulture);
        settings[Key("Width")] = _floating.Width.ToString(CultureInfo.InvariantCulture);
        settings[Key("Height")] = _floating.Height.ToString(CultureInfo.InvariantCulture);
        settings[Key("DockWidth")] = DockWidth.ToString(CultureInfo.InvariantCulture);
        settings[Key("DockHeight")] = DockHeight.ToString(CultureInfo.InvariantCulture);
    }

    internal void ShowTool() => Place(Side, _floating);

    private void HideTool()
    {
        if (Side == DockStyle.None) _floating = Bounds;
        CancelDrag();
        IsOpen = false;
        Hide();
        if (_showCommand is not null) _showCommand.Enabled = true;
        _editor?.PerformLayout();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_editor is { IsClosing: false } && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            HideTool();
        }
        base.OnFormClosing(e);
    }

    private void Place(DockStyle side, Rectangle floating)
    {
        if (_editor is null) return;
        bool docked = side != DockStyle.None;
        if (Side == DockStyle.None && IsOpen && docked) _floating = Bounds;
        if (TopLevel == docked)
        {
            Hide();
            Owner = null;
            Parent = null;
            TopLevel = !docked;
            FormBorderStyle = docked ? FormBorderStyle.None : FormBorderStyle.SizableToolWindow;
            if (docked) Parent = _editor;
        }
        Side = side;
        IsOpen = true;
        _header.Visible = _grip.Visible = docked;
        if (docked)
        {
            _lastSide = side;
            bool vertical = side is DockStyle.Left or DockStyle.Right;
            _grip.Dock = side switch
            {
                DockStyle.Left => DockStyle.Right,
                DockStyle.Right => DockStyle.Left,
                DockStyle.Top => DockStyle.Bottom,
                _ => DockStyle.Top,
            };
            if (vertical) _grip.Width = LogicalToDeviceUnits(5);
            else _grip.Height = LogicalToDeviceUnits(5);
            _grip.Cursor = vertical ? Cursors.VSplit : Cursors.HSplit;
            _header.SendToBack();
            _grip.SendToBack();
        }
        else
        {
            Owner = _editor;
            StartPosition = FormStartPosition.Manual;
            Bounds = _floating = FitToScreen(floating);
        }
        Show();
        _showCommand!.Enabled = false;
        _editor.PerformLayout();
    }

    private void ResizeDock(object? sender, MouseEventArgs e)
    {
        if (_resizeStart is not Point start || _editor is null) return;
        Point screen = _grip.PointToScreen(e.Location);
        int dx = screen.X - start.X, dy = screen.Y - start.Y;
        int min = LogicalToDeviceUnits(90);
        if (Side is DockStyle.Left or DockStyle.Right)
            DockWidth = Math.Clamp(_initialExtent + (Side == DockStyle.Left ? dx : -dx), min, Math.Max(min, _editor.ClientSize.Width));
        else
            DockHeight = Math.Clamp(_initialExtent + (Side == DockStyle.Top ? dy : -dy), min, Math.Max(min, _editor.ClientSize.Height));
        _editor.PerformLayout();
    }

    private static Rectangle FitToScreen(Rectangle bounds)
    {
        Rectangle work = Screen.FromRectangle(bounds).WorkingArea;
        int width = Math.Min(bounds.Width, work.Width), height = Math.Min(bounds.Height, work.Height);
        return new Rectangle(Math.Clamp(bounds.X, work.Left, work.Right - width),
            Math.Clamp(bounds.Y, work.Top, work.Bottom - height), width, height);
    }
}

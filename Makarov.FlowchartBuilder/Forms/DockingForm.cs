using System.ComponentModel;
using System.Globalization;

namespace Makarov.FlowchartBuilder.Forms;

/// <summary>
/// Tool window that snaps to the edges of the main window work area and keeps its position and docking
/// in the settings under "SecWndParams.&lt;RegistrySettingsName&gt;.*".
/// </summary>
public partial class DockingForm : Form
{
    private const int MinimumWindowSize = 100;

    public DockingForm()
    {
        InitializeComponent();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DockLeft { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DockRight { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DockTop { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DockBottom { get; set; }

    /// <summary>Name of this window in the settings; the class name by default.</summary>
    protected virtual string RegistrySettingsName => GetType().Name;

    private string SettingsPrefix => $"{Settings.Keys.SecondaryWindowParams}.{RegistrySettingsName}.";

    /// <summary>Snaps the left and top edges to the work area when the window is moved close to them.</summary>
    public void UpdateDockingWindowOnMove()
    {
        Rectangle work = Core.Instance.MainWindow.WorkRectangle;

        DockLeft = UpdateDock(DockLeft, work.Left - Left);
        if (Math.Abs(work.Left - Left) <= Settings.Environment.Docking.DockDistance)
            Left = work.Left;

        DockTop = UpdateDock(DockTop, work.Top - Top);
        if (Math.Abs(work.Top - Top) <= Settings.Environment.Docking.DockDistance)
            Top = work.Top;
    }

    /// <summary>Snaps the right and bottom edges to the work area when the window is resized close to them.</summary>
    public void UpdateDockingWindowOnResize()
    {
        Rectangle work = Core.Instance.MainWindow.WorkRectangle;

        DockRight = UpdateDock(DockRight, work.Right - Right);
        if (Math.Abs(work.Right - Right) <= Settings.Environment.Docking.DockDistance)
            Width += work.Right - Right;

        DockBottom = UpdateDock(DockBottom, work.Bottom - Bottom);
        if (Math.Abs(work.Bottom - Bottom) <= Settings.Environment.Docking.DockDistance)
            Height += work.Bottom - Bottom;
    }

    /// <summary>Keeps docked edges attached after the main window or its work area changed.</summary>
    public void UpdateDockedWindowPosition()
    {
        Rectangle work = Core.Instance.MainWindow.WorkRectangleWithScrollbars;

        if (DockLeft)
            Left = work.Left;

        if (DockTop)
            Top = work.Top;

        if (DockRight)
            Left = work.Right - Width;

        if (DockBottom)
            Top = work.Bottom - Height;
    }

    /// <summary>Docks within the dock distance, undocks beyond the undock distance and keeps the state in between.</summary>
    private static bool UpdateDock(bool docked, int distance) => Math.Abs(distance) switch
    {
        <= Settings.Environment.Docking.DockDistance => true,
        >= Settings.Environment.Docking.UndockDistance => false,
        _ => docked,
    };

    private void DockingForm_Load(object? sender, EventArgs e)
    {
        if (DesignMode)
            return;

        if (ReadInt("Left", 0) is { } left)
            Left = left;

        if (ReadInt("Top", 0) is { } top)
            Top = top;

        if (ReadInt("Width", MinimumWindowSize) is { } width)
            Width = width;

        if (ReadInt("Height", MinimumWindowSize) is { } height)
            Height = height;

        DockLeft = ReadBool("DockLeft") ?? DockLeft;
        DockRight = ReadBool("DockRight") ?? DockRight;
        DockTop = ReadBool("DockTop") ?? DockTop;
        DockBottom = ReadBool("DockBottom") ?? DockBottom;
    }

    private void DockingForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (DesignMode || !(Visible || Enabled))
            return;

        Write("Left", Left);
        Write("Top", Top);
        Write("Width", Width);
        Write("Height", Height);
        Write("DockLeft", DockLeft);
        Write("DockRight", DockRight);
        Write("DockTop", DockTop);
        Write("DockBottom", DockBottom);
    }

    private void DockingForm_Move(object? sender, EventArgs e)
    {
        if (DesignMode)
            return;

        UpdateDockingWindowOnMove();
        UpdateDockingWindowOnResize();
    }

    private void DockingForm_Resize(object? sender, EventArgs e)
    {
        if (!DesignMode)
            UpdateDockingWindowOnResize();
    }

    private int? ReadInt(string name, int minimum) =>
        int.TryParse(Core.Instance.SettingsStorage[SettingsPrefix + name], NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
            ? Math.Max(value, minimum)
            : null;

    private bool? ReadBool(string name) =>
        bool.TryParse(Core.Instance.SettingsStorage[SettingsPrefix + name], out bool value) ? value : null;

    private void Write(string name, int value) =>
        Core.Instance.SettingsStorage[SettingsPrefix + name] = value.ToString(CultureInfo.InvariantCulture);

    private void Write(string name, bool value) =>
        Core.Instance.SettingsStorage[SettingsPrefix + name] = value.ToString();
}

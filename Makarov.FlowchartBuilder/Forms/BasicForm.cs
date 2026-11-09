using System.Globalization;

namespace Makarov.FlowchartBuilder.Forms;

/// <summary>
/// Window with a main menu, a toolbar and a status bar; persists its position in the settings.
/// </summary>
public partial class BasicForm : Form
{
    private const int MinimumWindowSize = 100;

    public BasicForm()
    {
        InitializeComponent();
    }

    public MenuStrip MainMenu => mnuMain;

    public ToolStrip SecondaryMenu => mnuSecondary;

    public StatusStrip StatusMenu => mnuStatus;

    /// <summary>Client area between the toolbar and the status bar.</summary>
    public virtual Rectangle InnerRectangle =>
        new(
            ClientRectangle.Left,
            ClientRectangle.Top + SecondaryMenu.Top + SecondaryMenu.Height,
            ClientRectangle.Width - 2,
            ClientRectangle.Height - StatusMenu.Height - SecondaryMenu.Height - MainMenu.Height - 2);

    /// <summary><see cref="InnerRectangle"/> in screen coordinates.</summary>
    public Rectangle WorkRectangle
    {
        get
        {
            int borderWidth = (Width - ClientSize.Width) / 2;
            int titleBarHeight = Height - ClientSize.Height - borderWidth;
            Rectangle inner = InnerRectangle;

            return new Rectangle(Left + inner.Left + borderWidth, Top + inner.Top + titleBarHeight, inner.Width, inner.Height);
        }
    }

    protected void LoadWindowParamsFromRegistry()
    {
        if (ReadSetting(Settings.Keys.MainWindow.X, 0) is { } x)
            Left = x;

        if (ReadSetting(Settings.Keys.MainWindow.Y, 0) is { } y)
            Top = y;

        if (ReadSetting(Settings.Keys.MainWindow.Width, MinimumWindowSize) is { } width)
            Width = width;

        if (ReadSetting(Settings.Keys.MainWindow.Height, MinimumWindowSize) is { } height)
            Height = height;

        if (Enum.TryParse(Core.Instance.SettingsStorage[Settings.Keys.MainWindow.WindowState], out FormWindowState state) &&
            state != FormWindowState.Normal)
        {
            WindowState = state;
        }
    }

    protected void SaveWindowParamsToRegistry()
    {
        var storage = Core.Instance.SettingsStorage;

        if (WindowState == FormWindowState.Normal)
        {
            storage[Settings.Keys.MainWindow.X] = Left.ToString(CultureInfo.InvariantCulture);
            storage[Settings.Keys.MainWindow.Y] = Top.ToString(CultureInfo.InvariantCulture);
            storage[Settings.Keys.MainWindow.Width] = Width.ToString(CultureInfo.InvariantCulture);
            storage[Settings.Keys.MainWindow.Height] = Height.ToString(CultureInfo.InvariantCulture);
        }

        storage[Settings.Keys.MainWindow.WindowState] = WindowState.ToString();
    }

    /// <summary>Reads a stored integer, raising it to <paramref name="minimum"/> if needed.</summary>
    private static int? ReadSetting(string key, int minimum) =>
        int.TryParse(Core.Instance.SettingsStorage[key], NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
            ? Math.Max(value, minimum)
            : null;
}

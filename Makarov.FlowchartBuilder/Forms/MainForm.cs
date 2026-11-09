using Makarov.Framework.Components;

namespace Makarov.FlowchartBuilder.Forms;

/// <summary>
/// Main window: menus, document tabs and the work area painted by <see cref="Core.Paint"/>.
/// </summary>
public partial class MainForm : BasicForm
{
    public MainForm()
    {
        InitializeComponent();

        SetStyle(
            ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
            true);
    }

    /// <summary>Set once the main window starts closing, so that tool windows close instead of hiding.</summary>
    public bool IsClosing { get; private set; }

    /// <summary>State to restore when the window is brought back from the taskbar.</summary>
    public FormWindowState WindowStateBeforeMinimize { get; private set; } = FormWindowState.Normal;

    public HScrollBar HorizontalScrollBar => hScrollBar;

    public VScrollBar VerticalScrollBar => vScrollBar;

    public TabsPanel DocumentsTabs => documentsTabsBar;

    /// <summary>Work area: the client area below the document tabs, without the rulers.</summary>
    public override Rectangle InnerRectangle
    {
        get
        {
            int rulersSize = Settings.Environment.Rulers ? Settings.Environment.RulersSize : 0;
            int tabsHeight = documentsTabsBar.Height;
            Rectangle rect = base.InnerRectangle;

            return new Rectangle(
                rect.Left + rulersSize,
                rect.Top + rulersSize + tabsHeight,
                rect.Width - rulersSize,
                rect.Height - rulersSize - tabsHeight);
        }
    }

    /// <summary><see cref="BasicForm.WorkRectangle"/> without the visible scroll bars.</summary>
    public Rectangle WorkRectangleWithScrollbars
    {
        get
        {
            Rectangle rect = WorkRectangle;
            return new Rectangle(
                rect.Left,
                rect.Top,
                rect.Width - (vScrollBar.Visible ? vScrollBar.Width : 0),
                rect.Height - (hScrollBar.Visible ? hScrollBar.Height : 0));
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Core.Instance.Paint(e.Graphics);
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        Text = Core.Instance.CurrentTranslator["FlowchartBuilder_Caption"];

        if (Core.Instance.IsFirstRun)
        {
            SetBounds(50, 50, 800, 600);
            WindowState = FormWindowState.Maximized;
        }

        LoadWindowParamsFromRegistry();

        MenuLoader.LoadMainMenu(this);
        MenuLoader.LoadSecondaryMenu(this);

        // Documents passed on the command line were opened before the menus existed.
        Core.Instance.ActiveSheet?.UpdateGlyphsCommands();

        Core.Instance.GetWindowAsChild<GlyphsForm>().Show();
        Core.Instance.GetWindowAsChild<PropertiesForm>().Show();

        DocumentsTabs.SelectedTabChanged += (_, _) =>
        {
            if (DocumentsTabs.SelectedTab is { } tab)
                Core.Instance.SelectDocument(tab.Id);
        };
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        if (Settings.Environment.ShowNewSheetDialogOnStartup && !Core.Instance.HasDocuments)
        {
            using var dialog = new SheetsForm();
            dialog.ShowDialog(this);
        }

        if (Core.Instance.IsFirstRun)
            Core.Instance.SetDefaultWindowsPos();

        Core.Instance.Redraw();
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (IsClosing)
            return;

        SaveWindowParamsToRegistry();
        IsClosing = true;

        foreach (Form window in Core.Instance.Windows.Windows.Where(window => window != this))
            window.Close();
    }

    private void MainForm_Resize(object? sender, EventArgs e)
    {
        if (WindowState != FormWindowState.Minimized)
            WindowStateBeforeMinimize = WindowState;

        UpdateDockedWindows();

        if (Visible)
            Core.Instance.Redraw();
    }

    private void MainForm_Move(object? sender, EventArgs e) => UpdateDockedWindows();

    private void MainForm_MouseDown(object? sender, MouseEventArgs e) =>
        Core.Instance.ActiveSheet?.MouseDown(Core.Instance.ClientToSheet(e.Location));

    private void MainForm_MouseMove(object? sender, MouseEventArgs e) =>
        Core.Instance.ActiveSheet?.MouseMove(Core.Instance.ClientToSheet(e.Location));

    private void MainForm_MouseUp(object? sender, MouseEventArgs e) =>
        Core.Instance.ActiveSheet?.MouseUp();

    private void hScrollBar_Scroll(object? sender, ScrollEventArgs e) => Core.Instance.Redraw();

    private void vScrollBar_Scroll(object? sender, ScrollEventArgs e) => Core.Instance.Redraw();

    private static void UpdateDockedWindows()
    {
        foreach (DockingForm window in Core.Instance.Windows.Windows.OfType<DockingForm>())
            window.UpdateDockedWindowPosition();
    }
}

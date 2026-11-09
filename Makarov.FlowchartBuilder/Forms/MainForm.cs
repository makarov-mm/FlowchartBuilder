using Makarov.FlowchartBuilder.Commands;
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

        // Thin edges around the view show the work area color; the view lies under the scroll bars.
        BackColor = Settings.Colors.WorkArea;
        sheetView.SendToBack();
        documentsTabsBar.TabCloseRequested += (_, e) =>
        {
            if (Core.Instance.Documents.FirstOrDefault(document => document.Id == e.Tab.Id) is { } document)
                Core.Instance.CloseDocument(document);
        };
    }

    /// <summary>Set once the main window starts closing, so that tool windows close instead of hiding.</summary>
    public bool IsClosing { get; private set; }

    /// <summary>State to restore when the window is brought back from the taskbar.</summary>
    public FormWindowState WindowStateBeforeMinimize { get; private set; } = FormWindowState.Normal;

    public HScrollBar HorizontalScrollBar => hScrollBar;

    public VScrollBar VerticalScrollBar => vScrollBar;

    public TabsPanel DocumentsTabs => documentsTabsBar;

    public SheetView SheetView => sheetView;

    /// <summary>Work area: the part of the sheet view not covered by the rulers, in client coordinates.</summary>
    public override Rectangle InnerRectangle
    {
        get
        {
            int rulersSize = Settings.Environment.Rulers ? Settings.Environment.RulersSize : 0;
            Rectangle view = ViewBounds;

            return new Rectangle(view.Left + rulersSize, view.Top + rulersSize, view.Width - rulersSize, view.Height - rulersSize);
        }
    }

    /// <summary>The client area below the document tabs, between the toolbar and the status bar.</summary>
    private Rectangle ViewBounds
    {
        get
        {
            Rectangle rect = base.InnerRectangle;
            int tabsHeight = documentsTabsBar.Height;
            return new Rectangle(rect.Left, rect.Top + tabsHeight, rect.Width, Math.Max(0, rect.Height - tabsHeight));
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

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);

        if (sheetView is not null)
            sheetView.Bounds = ViewBounds;
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

        // Documents passed on the command line were opened before the window and its menus were ready.
        Core.Instance.RefreshDocumentState();

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

        if (!Core.Instance.ConfirmCloseAll())
        {
            e.Cancel = true;
            return;
        }

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

    private void sheetView_MouseDown(object? sender, MouseEventArgs e)
    {
        sheetView.Focus();

        if (e.Button == MouseButtons.Left)
            Core.Instance.ActiveSheet?.MouseDown(Core.Instance.ClientToSheet(e.Location));
    }

    private void sheetView_MouseMove(object? sender, MouseEventArgs e) =>
        Core.Instance.ActiveSheet?.MouseMove(Core.Instance.ClientToSheet(e.Location));

    private void sheetView_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            Core.Instance.ActiveSheet?.MouseUp();
    }

    /// <summary>The wheel scrolls the sheet (with Shift horizontally); with Ctrl it zooms.</summary>
    private void sheetView_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (!Core.Instance.HasDocuments || e.Delta == 0)
            return;

        if ((ModifierKeys & Keys.Control) != 0)
        {
            Command command = e.Delta > 0 ? Command.GetInstance<ZoomInCommand>() : Command.GetInstance<ZoomOutCommand>();
            command.Execute();
            return;
        }

        ScrollBar scrollBar = (ModifierKeys & Keys.Shift) != 0 ? hScrollBar : vScrollBar;
        if (!scrollBar.Visible)
            return;

        int maximum = scrollBar.Maximum - scrollBar.LargeChange + 1;
        scrollBar.Value = Math.Clamp(scrollBar.Value - Math.Sign(e.Delta) * scrollBar.SmallChange, scrollBar.Minimum, maximum);
        Core.Instance.Redraw();
    }

    private void hScrollBar_Scroll(object? sender, ScrollEventArgs e) => Core.Instance.Redraw();

    private void vScrollBar_Scroll(object? sender, ScrollEventArgs e) => Core.Instance.Redraw();

    private static void UpdateDockedWindows()
    {
        foreach (DockingForm window in Core.Instance.Windows.Windows.OfType<DockingForm>())
            window.UpdateDockedWindowPosition();
    }
}

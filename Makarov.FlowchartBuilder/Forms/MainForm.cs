using Makarov.FlowchartBuilder.Commands;
using Makarov.Framework.Components;

namespace Makarov.FlowchartBuilder.Forms;

/// <summary>
/// Main window: menus, document tabs and the work area painted by <see cref="Core.Paint"/>.
/// </summary>
public partial class MainForm : BasicForm
{
    private Point? _panStart;

    public MainForm()
    {
        InitializeComponent();

        BackColor = Settings.Colors.WorkArea;
        sheetView.SendToBack();
        sheetView.MouseCaptureChanged += (_, _) => { if (!sheetView.Capture) EndPan(); };
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
        if (!Core.Instance.HasDocuments)
            Command.GetInstance<NewCommand>().Run();

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

        if (e.Button == MouseButtons.Middle && Core.Instance.ActiveSheet is not null)
        {
            _panStart = e.Location;
            sheetView.Capture = true;
            sheetView.Cursor = Cursors.Hand;
        }
        else if (e.Button == MouseButtons.Left && _panStart is null)
            Core.Instance.ActiveSheet?.MouseDown(Core.Instance.ClientToSheet(e.Location));
    }

    private void sheetView_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_panStart is Point start)
        {
            Pan(start.X - e.X, start.Y - e.Y);
            _panStart = e.Location;
        }
        else
            Core.Instance.ActiveSheet?.MouseMove(Core.Instance.ClientToSheet(e.Location));
    }

    private void sheetView_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Middle)
        {
            EndPan();
            sheetView.Capture = false;
        }
        else if (e.Button == MouseButtons.Left)
            Core.Instance.ActiveSheet?.MouseUp();
    }

    private void EndPan()
    {
        if (_panStart is null) return;
        _panStart = null;
        sheetView.Cursor = Cursors.Default;
    }

    private static void Pan(int dx, int dy)
    {
        if (Core.Instance.ActiveSheet is not { } sheet) return;
        sheet.ViewOrigin = new PointF(sheet.ViewOrigin.X + dx / sheet.ScaleFactor, sheet.ViewOrigin.Y + dy / sheet.ScaleFactor);
        Core.Instance.Redraw();
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

        int distance = -e.Delta / 3;
        if ((ModifierKeys & Keys.Shift) != 0) Pan(distance, 0);
        else Pan(0, distance);
    }

    private static void UpdateDockedWindows()
    {
        foreach (DockingForm window in Core.Instance.Windows.Windows.OfType<DockingForm>())
            window.UpdateDockedWindowPosition();
    }
}

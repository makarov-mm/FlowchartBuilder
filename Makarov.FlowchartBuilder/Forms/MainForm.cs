using System.ComponentModel;
using System;
using System.Drawing;
using System.Windows.Forms;
using Makarov.Framework.Components;

namespace Makarov.FlowchartBuilder.Forms
{
    public partial class MainForm : BasicForm
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void FBMainWindow_Load(object sender, EventArgs e)
        {

            Text = Core.Instance.CurrentTranslator["FlowchartBuilder_Caption"];

            if (Core.Instance.IsFirstRun)
            {
                Left = 50;
                Top = 50;
                Width = 800;
                Height = 600;
                WindowState = FormWindowState.Maximized;
            }

            LoadWindowParamsFromRegistry();

            MenuLoader.LoadMainMenu(this);

            MenuLoader.LoadSecondaryMenu(this);

            Core.Instance.GetWindowAsChild<GlyphsForm>().Show();

            Core.Instance.GetWindowAsChild<PropertiesForm>().Show();

            DocumentsTabs.SelectedTabChanged += (s, args) =>
                                                    {
                                                        var tab = DocumentsTabs.SelectedTab;

                                                        Core.Instance.SelectDocument(tab.Id);
                                                    };
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsClosing
        {
            get; set;
        }

        private void FBMainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsClosing)
                return;

            SaveWindowParamsToRegistry();
            IsClosing = true;

            foreach (var wnd in Core.Instance.Windows.Windows)
                if (wnd != this)
                    wnd.Close();

            Close();
        }

        private void FBMainWindow_Paint(object sender, PaintEventArgs e)
        {
            Core.Instance.Redraw();
        }

        private void FBMainWindow_Resize(object sender, EventArgs e)
        {

            foreach (var wnd in Core.Instance.Windows.Windows)
                if (wnd is DockingForm)
                {
                    var dockingWnd = (DockingForm)wnd;

                    dockingWnd.UpdateDockedWindowPosition();
                }

            if (Visible)
                Core.Instance.Redraw();
        }

        public override Rectangle InnerRectangle
        {
            get
            {
                int rulersSize = Settings.Environment.Rulers ? Settings.Environment.RulersSize : 0;
                int docsBarHeight = documentsTabsBar.Height;
                Rectangle rect = base.InnerRectangle;
                return new Rectangle(
                    rect.Left + rulersSize,
                    rect.Top + rulersSize + docsBarHeight,
                    rect.Width - rulersSize,
                    rect.Height - rulersSize - docsBarHeight);
            }
        }

        private void FBMainWindow_Shown(object sender, EventArgs e)
        {
            if (!Core.Instance.IsDocumentsExists)
                Core.Instance.AddDocument(new Document());

            if (Core.Instance.IsFirstRun)
                Core.Instance.SetDefaultWindowsPos();

            Core.Instance.Redraw();
        }

        private void FBMainWindow_Enter(object sender, EventArgs e)
        {

            Core.Instance.Redraw();
        }

        private Point? _panStart;

        private Point CoordinatesInSheet(Point client)
        {
            Point origin = Core.Instance.CurrentDocument.DocumentSheet.ScaledViewOrigin;
            return new Point(client.X - InnerRectangle.Left + origin.X,
                             client.Y - InnerRectangle.Top + origin.Y);
        }

        private void FBMainWindow_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Core.Instance.IsDocumentsExists || !InnerRectangle.Contains(e.Location))
                return;

            if (e.Button == MouseButtons.Middle)
            {
                _panStart = e.Location;
                Capture = true;
                Cursor = Cursors.Hand;
            }
            else if (e.Button == MouseButtons.Left && !_panStart.HasValue)
                Core.Instance.CurrentDocument.DocumentSheet.MouseDown(CoordinatesInSheet(e.Location));
        }

        private void FBMainWindow_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle && _panStart.HasValue)
            {
                _panStart = null;
                Capture = false;
                Cursor = Cursors.Default;
            }
            else if (e.Button == MouseButtons.Left && Core.Instance.IsDocumentsExists)
                Core.Instance.CurrentDocument.DocumentSheet.MouseUp();
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            if (!Capture && _panStart.HasValue)
            {
                _panStart = null;
                Cursor = Cursors.Default;
            }
        }

        private void FBMainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Core.Instance.IsDocumentsExists) return;
            var sheet = Core.Instance.CurrentDocument.DocumentSheet;
            if (_panStart is Point start)
            {
                sheet.ViewOrigin = new PointF(
                    sheet.ViewOrigin.X + (start.X - e.X) / sheet.ScaleFactor,
                    sheet.ViewOrigin.Y + (start.Y - e.Y) / sheet.ScaleFactor);
                _panStart = e.Location;
                Core.Instance.Redraw();
            }
            else
                sheet.MouseMove(CoordinatesInSheet(e.Location));
        }

        private void FBMainWindow_Move(object sender, EventArgs e)
        {

            foreach (var wnd in Core.Instance.Windows.Windows)
                if (wnd is DockingForm)
                {
                    var dockingWnd = (DockingForm)wnd;

                    dockingWnd.UpdateDockedWindowPosition();
                }
        }

        public FormWindowState OldWindowState
        {
            get; private set;
        }

        public TabsPanel DocumentsTabs
        {
            get { return documentsTabsBar; }
        }

        protected override void DefWndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020;

            if (m.Msg == WM_SYSCOMMAND && m.WParam == (IntPtr)SC_MINIMIZE)
            {
                OldWindowState = WindowState;
            }

            base.DefWndProc(ref m);
        }
    }
}

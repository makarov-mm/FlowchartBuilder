using System.ComponentModel;
using System;
using System.Windows.Forms;
using BaseSettings = Makarov.Framework.Core.Settings;

namespace Makarov.FlowchartBuilder.Forms
{
    public partial class DockingForm : Form
    {
        public DockingForm()
        {
            InitializeComponent();
        }

        public string ClassName
        {
            get
            {
                string[] clsType = GetType().Name.Split('.');
                return clsType[clsType.Length - 1];
            }
        }

        protected string RegistrySettingsKey
        {
            get
            {
                return Settings.RegistryKeys.SecondaryWindowParams + "." + RegistrySettingsName;
            }
        }

        protected virtual string RegistrySettingsName
        {
            get { throw new NotImplementedException(); }
        }

        private void DockingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Settings.Environment.InDesignTime) return;

            if (!(Visible || Enabled))
                return;

            Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".Left"] = Left.ToString();
            Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".Top"] = Top.ToString();
            Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".Width"] = Width.ToString();
            Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".Height"] = Height.ToString();

            Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".DockLeft"] = DockLeft.ToString();
            Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".DockRight"] = DockRight.ToString();
            Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".DockTop"] = DockTop.ToString();
            Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".DockBottom"] = DockBottom.ToString();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DockLeft
        {
            get;
            set;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DockRight
        {
            get;
            set;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DockTop
        {
            get;
            set;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DockBottom
        {
            get;
            set;
        }

        private void DockingForm_Load(object sender, EventArgs e)
        {
            if (Settings.Environment.InDesignTime) return;

            string x = Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".Left"];
            string y = Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".Top"];
            string width = Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".Width"];
            string height = Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".Height"];

            if (!string.IsNullOrEmpty(x))
            {
                int ix = int.Parse(x);
                if (ix < 0)
                {
                    ix = 0;
                    Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".Left"] = ix.ToString();
                }
                Left = ix;
            }

            if (!string.IsNullOrEmpty(y))
            {
                int iy = int.Parse(y);
                if (iy < 0)
                {
                    iy = 0;
                    Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".Top"] = iy.ToString();
                }
                Top = iy;
            }

            if (!string.IsNullOrEmpty(width))
            {
                int iwidth = int.Parse(width);
                if (iwidth < 100)
                {
                    iwidth = 100;
                    Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".Width"] = iwidth.ToString();
                }
                Width = iwidth;
            }

            if (!string.IsNullOrEmpty(height))
            {
                int iheight = int.Parse(height);
                if (iheight < 100)
                {
                    iheight = 100;
                    Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".Height"] = iheight.ToString();
                }
                Height = iheight;
            }

            string dockLeft = Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".DockLeft"];
            string dockRight = Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".DockRight"];
            string dockTop = Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".DockTop"];
            string dockBottom = Core.Instance.CachedSettingsStorage[RegistrySettingsKey + ".DockBottom"];

            if (!string.IsNullOrEmpty(dockLeft))
                DockLeft = bool.Parse(dockLeft);

            if (!string.IsNullOrEmpty(dockRight))
                DockRight = bool.Parse(dockRight);

            if (!string.IsNullOrEmpty(dockTop))
                DockTop = bool.Parse(dockTop);

            if (!string.IsNullOrEmpty(dockBottom))
                DockBottom = bool.Parse(dockBottom);
        }

        public void UpdateDockingWindowOnMove()
        {

            if (Math.Abs(Core.Instance.MainWindow.WorkRectangle.Left - Left) <= Settings.Environment.Docking.DockDistance)
            {
                Left = Core.Instance.MainWindow.WorkRectangle.Left;
                DockLeft = true;
            }
            else if (Math.Abs(Core.Instance.MainWindow.WorkRectangle.Left - Left) >= Settings.Environment.Docking.UndockDistance)
                DockLeft = false;

            if (Math.Abs(Core.Instance.MainWindow.WorkRectangle.Top - Top) <= Settings.Environment.Docking.DockDistance)
            {
                Top = Core.Instance.MainWindow.WorkRectangle.Top;
                DockTop = true;
            }
            else if (Math.Abs(Core.Instance.MainWindow.WorkRectangle.Top - Top) >= Settings.Environment.Docking.UndockDistance)
                DockTop = false;
        }

        public void UpdateDockingWindowOnResize()
        {

            if (Math.Abs(Core.Instance.MainWindow.WorkRectangle.Right - (Left + Width)) <= Settings.Environment.Docking.DockDistance)
            {
                Width += Core.Instance.MainWindow.WorkRectangle.Right - (Left + Width);
                DockRight = true;
            }
            else if (Math.Abs(Core.Instance.MainWindow.WorkRectangle.Right - (Left + Width)) >= Settings.Environment.Docking.UndockDistance)
                DockRight = false;

            if (Math.Abs(Core.Instance.MainWindow.WorkRectangle.Bottom - (Top + Height)) <= Settings.Environment.Docking.DockDistance)
            {
                Height += Core.Instance.MainWindow.WorkRectangle.Bottom - (Top + Height);
                DockBottom = true;
            }
            else if (Math.Abs(Core.Instance.MainWindow.WorkRectangle.Bottom - (Top + Height)) >= Settings.Environment.Docking.UndockDistance)
                DockBottom = false;
        }

        public void UpdateDockedWindowPosition()
        {
            if (DockLeft)
                Left = Core.Instance.MainWindow.WorkRectangleWithScrollbars.Left;

            if (DockTop)
                Top = Core.Instance.MainWindow.WorkRectangleWithScrollbars.Top;

            if (DockRight)
                Left = Core.Instance.MainWindow.WorkRectangleWithScrollbars.Right - Width;

            if (DockBottom)
                Top = Core.Instance.MainWindow.WorkRectangleWithScrollbars.Bottom - Height;
        }

        private void DockingForm_Move(object sender, EventArgs e)
        {
            if (Settings.Environment.InDesignTime) return;

            UpdateDockingWindowOnMove();
            UpdateDockingWindowOnResize();
        }

        private void DockingForm_Resize(object sender, EventArgs e)
        {
            if (Settings.Environment.InDesignTime) return;

            UpdateDockingWindowOnResize();
        }
    }
}

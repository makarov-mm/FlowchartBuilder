using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Makarov.Framework.Core;

namespace Makarov.Framework.Components
{

    public class TabsPanel : Control
    {
        #region Exceptions

        public class TabsPanelException : MakarovFrameworkException
        {

            public TabsPanelException(string message)
                : base(message ?? string.Empty)
            { }
        }
        #endregion

        #region Private members

        private readonly List<Tab> _tabs = new List<Tab>();

        private int _tabWidth = 100;

        private int _tabHeight = 20;

        private int? _selectedIndex;
        #endregion

        #region Constructors
        public TabsPanel()
        {
            Offset = 2;
            CornerSize = 6;
            SelectedIndex = null;
        }
        #endregion

        #region Public methods
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int i = 0;

            foreach (KeyValuePair<string, GraphicsPath> kvp in Pathes)
            {
                bool isActive = SelectedIndex.HasValue && i == SelectedIndex.Value;

                using (kvp.Value)
                {
                    e.Graphics.TranslateTransform(kvp.Value.GetBounds().Width, 0);
                }

                ++i;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            int i = 0;

            foreach (KeyValuePair<string, GraphicsPath> kvp in Pathes)
            {
                using (kvp.Value)
                {
                    RectangleF bounds = kvp.Value.GetBounds();

                    if (bounds.Contains(e.X, e.Y))
                    {
                        SelectedIndex = i;
                        Invalidate();
                    }
                }

                ++i;
            }
        }

        public void AddTab(string tab, int id)
        {
            _tabs.Add(new Tab(id, tab));
            SelectedIndex = _tabs.Count - 1;
            Invalidate();
        }
        #endregion

        #region Properties

        [DefaultValue(100)]
        public int TabWidth
        {
            get => _tabWidth;

            set
            {
                if (value <= 0)
                    throw new TabsPanelException(
                        $"Invalid tab width: {value}.");

                _tabWidth = value;
            }
        }

        [DefaultValue(20)]
        public int TabHeight
        {
            get => _tabHeight;

            set
            {
                if (value <= 0)
                    throw new TabsPanelException(
                        $"Invalid tab height: {value}");

                _tabHeight = value;
            }
        }

        public Tab SelectedTab
        {
            get
            {
                int? idx = SelectedIndex;

                if (idx.HasValue)
                    return _tabs[idx.Value];

                return null;
            }
        }

        public int? SelectedIndex
        {
            get { return _selectedIndex; }

            private set
            {
                if (_selectedIndex.HasValue != value.HasValue ||
                    (_selectedIndex.HasValue && value.HasValue && _selectedIndex.Value != value.Value))
                {
                    _selectedIndex = value;

                    if (SelectedTabChanged != null)
                        SelectedTabChanged(this, EventArgs.Empty);
                }
                else
                {
                    _selectedIndex = value;
                }
            }
        }

        [DefaultValue(2)]
        public int Offset { get; set; }

        [DefaultValue(6)]
        public int CornerSize { get; set; }

        public event EventHandler SelectedTabChanged;

        private IEnumerable<KeyValuePair<string, GraphicsPath>> Pathes
        {
            get
            {
                var list = new List<KeyValuePair<string, GraphicsPath>>();

                int i = 0;

                using (Graphics gfx = CreateGraphics())
                using (var font = new Font("Arial", 8))
                    foreach (Tab tab in _tabs)
                    {

                        var path = tab.CreatePath(gfx, font, 200, null, Height, Height);

                        list.Add(new KeyValuePair<string, GraphicsPath>(tab.Caption, path));
                        ++i;
                    }

                return list.ToArray();
            }
        }
        #endregion
    }
}

using System.ComponentModel;

namespace Makarov.Framework.Components;

/// <summary>
/// Horizontal strip of document tabs; clicking a tab selects it and raises <see cref="SelectedTabChanged"/>.
/// </summary>
public class TabsPanel : Control
{
    private const int TextPadding = 16;

    private readonly List<Tab> _tabs = [];
    private int _selectedIndex = -1;
    private int _minimumTabWidth = 200;

    public TabsPanel()
    {
        SetStyle(
            ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw,
            true);
    }

    public event EventHandler? SelectedTabChanged;

    [DefaultValue(200)]
    public int MinimumTabWidth
    {
        get => _minimumTabWidth;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            _minimumTabWidth = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<Tab> Tabs => _tabs;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Tab? SelectedTab => _selectedIndex >= 0 ? _tabs[_selectedIndex] : null;

    /// <summary>Index of the selected tab, or -1 when there is none.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, -1);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, _tabs.Count);

            if (_selectedIndex == value)
                return;

            _selectedIndex = value;
            Invalidate();
            SelectedTabChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AddTab(string caption, int id)
    {
        _tabs.Add(new Tab(id, caption));
        SelectedIndex = _tabs.Count - 1;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        int left = 0;
        for (int i = 0; i < _tabs.Count; i++)
        {
            int width = TabWidth(_tabs[i]);
            _tabs[i].Draw(e.Graphics, new Rectangle(left, 0, width, Height - 1), Font, i == _selectedIndex);
            left += width;
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        int left = 0;
        for (int i = 0; i < _tabs.Count; i++)
        {
            int width = TabWidth(_tabs[i]);
            if (e.X >= left && e.X < left + width)
            {
                SelectedIndex = i;
                return;
            }

            left += width;
        }
    }

    private int TabWidth(Tab tab) => Math.Max(MinimumTabWidth, tab.MeasureWidth(Font) + TextPadding);
}

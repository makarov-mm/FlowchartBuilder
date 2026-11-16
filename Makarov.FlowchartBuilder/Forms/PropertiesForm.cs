using System.ComponentModel;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Forms;

/// <summary>
/// Property grid for the selected glyphs; it shows only the document properties (<see cref="ActivePropertyAttribute"/>).
/// </summary>
public partial class PropertiesForm : DockingForm
{
    public PropertiesForm()
    {
        InitializeComponent();
        gridView.BrowsableAttributes = new AttributeCollection(new ActivePropertyAttribute(), BrowsableAttribute.Yes);
    }

    public PropertyGrid Grid => gridView;

    private void PropertiesForm_Load(object? sender, EventArgs e) =>
        Text = Core.Instance.CurrentTranslator["Wnd_Properties_Title"];

    private void gridView_PropertyValueChanged(object? sender, PropertyValueChangedEventArgs e)
    {
        Core.Instance.CommitChange();
        Core.Instance.Redraw();
    }
}

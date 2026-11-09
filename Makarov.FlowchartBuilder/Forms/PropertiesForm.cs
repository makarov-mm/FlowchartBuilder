using System.ComponentModel;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.FlowchartBuilder.Commands;

namespace Makarov.FlowchartBuilder.Forms;

/// <summary>
/// Property grid for the selected glyphs; it shows only the document properties (<see cref="ActivePropertyAttribute"/>).
/// </summary>
public partial class PropertiesForm : DockingForm
{
    public PropertiesForm()
    {
        InitializeComponent();
        gridView.BrowsableAttributes = new AttributeCollection(new ActivePropertyAttribute());
    }

    public PropertyGrid Grid => gridView;

    private void PropertiesForm_Load(object? sender, EventArgs e) =>
        Text = Core.Instance.CurrentTranslator["Wnd_Properties_Title"];

    private void PropertiesForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (Core.Instance.MainWindow.IsClosing)
            return;

        e.Cancel = true;
        Hide();
        Command.GetInstance<PropertiesWindowCommand>().Enabled = true;
    }

    private void gridView_PropertyValueChanged(object? sender, PropertyValueChangedEventArgs e) => Core.Instance.Redraw();
}

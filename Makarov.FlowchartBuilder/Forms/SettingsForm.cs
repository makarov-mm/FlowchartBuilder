using Makarov.Framework.Core;

namespace Makarov.FlowchartBuilder.Forms;

/// <summary>
/// "Other settings" dialog.
/// </summary>
public partial class SettingsForm : Form
{
    public SettingsForm()
    {
        InitializeComponent();
    }

    private void SettingsForm_Load(object? sender, EventArgs e)
    {
        Translator translator = Core.Instance.CurrentTranslator;

        Text = translator["Wnd_OtherSettings"];
        tabPage1.Text = translator["Tab_Common"];
        cbAntialiasing.Text = translator["Cb_Antialiasing"];
        btnOk.Text = translator["OK"];
        btnCancel.Text = translator["Cancel"];

        cbAntialiasing.Checked = Settings.Environment.Antialiasing;
    }

    private void btnOk_Click(object? sender, EventArgs e)
    {
        Settings.Environment.Antialiasing = cbAntialiasing.Checked;
        Core.Instance.Redraw();
        Close();
    }

    private void btnCancel_Click(object? sender, EventArgs e) => Close();
}

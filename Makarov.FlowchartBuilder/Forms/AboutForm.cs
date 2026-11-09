using Makarov.Framework.Core;

namespace Makarov.FlowchartBuilder.Forms;

public partial class AboutForm : Form
{
    public AboutForm()
    {
        InitializeComponent();
    }

    private void AboutForm_Load(object? sender, EventArgs e)
    {
        Translator translator = Core.Instance.CurrentTranslator;

        Text = translator["Wnd_About_Title"];
        lblVersion.Text = translator["Wnd_About_VersionLabel"] + Settings.Environment.Version;
        lblCopyright.Text = translator["Wnd_About_CopyrightLabel"];
        txtLicense.Text = translator["License_Text"];
        btnOk.Text = translator["Close"];
    }

    private void btnOk_Click(object? sender, EventArgs e) => Close();
}

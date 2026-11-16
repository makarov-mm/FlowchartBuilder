using System.Reflection;

namespace Makarov.FlowchartBuilder.Forms;

public sealed class AboutForm : Form
{
    public AboutForm()
    {
        var t = Core.Instance.CurrentTranslator;
        var assembly = typeof(AboutForm).Assembly;
        Text = t["Wnd_About_Title"];
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(540, 380);
        MinimumSize = new Size(420, 300);
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = MinimizeBox = MaximizeBox = false;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 5 };
        foreach (var mode in new[] { SizeType.AutoSize, SizeType.AutoSize, SizeType.AutoSize, SizeType.Percent, SizeType.AutoSize })
            layout.RowStyles.Add(new RowStyle(mode, mode == SizeType.Percent ? 100 : 0));
        layout.Controls.Add(new Label { Text = "FlowchartBuilder", AutoSize = true, Font = new Font(Font.FontFamily, 16, FontStyle.Bold) });
        layout.Controls.Add(new Label { Text = t["Wnd_About_VersionLabel"].TrimEnd() + " " + Settings.Environment.Version, AutoSize = true });
        layout.Controls.Add(new Label { Text = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright, AutoSize = true });
        using var reader = new StreamReader(assembly.GetManifestResourceStream("FlowchartBuilder.License")!);
        layout.Controls.Add(new TextBox { Text = reader.ReadToEnd().ReplaceLineEndings(), Multiline = true, ReadOnly = true,
            ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, Margin = new Padding(0, 12, 0, 8) });
        var close = new Button { Text = t["Close"], AutoSize = true, Anchor = AnchorStyles.Right, DialogResult = DialogResult.OK };
        layout.Controls.Add(close);
        Controls.Add(layout);
        AcceptButton = CancelButton = close;
    }
}

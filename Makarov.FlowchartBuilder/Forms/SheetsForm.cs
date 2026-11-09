using Makarov.FlowchartBuilder.Commands;
using Makarov.FlowchartBuilder.Sheets;
using Makarov.Framework.Core;

namespace Makarov.FlowchartBuilder.Forms;

/// <summary>
/// "New document" dialog: lists the paper formats (sheet types) grouped by family.
/// </summary>
public partial class SheetsForm : Form
{
    public SheetsForm()
    {
        InitializeComponent();

        Translator translator = Core.Instance.CurrentTranslator;
        Text = translator["Command_New"].TrimEnd('.');
        btnOk.Text = translator["OK"];
        btnCancel.Text = translator["Cancel"];
        cbShowOnStartup.Checked = Settings.Environment.ShowNewSheetDialogOnStartup;

        FillSheetTypes(translator);
    }

    private void FillSheetTypes(Translator translator)
    {
        var images = new ImageList
        {
            ImageSize = new Size(Sheet.ThumbnailWidth, Sheet.ThumbnailHeight),
            ColorDepth = ColorDepth.Depth32Bit,
        };

        lstView.LargeImageList = images;
        lstView.ForeColor = Settings.Colors.ThumbnailText;

        IEnumerable<Sheet> sheets = typeof(Sheet).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.IsSubclassOf(typeof(Sheet)))
            .Select(type => (Sheet)Activator.CreateInstance(type)!);

        foreach (IGrouping<string, Sheet> family in sheets.GroupBy(sheet => sheet.Family).OrderBy(family => family.Key, StringComparer.CurrentCulture))
        {
            ListViewGroup group = lstView.Groups.Add(family.Key, translator[family.Key]);

            foreach (Sheet sheet in family.OrderBy(sheet => sheet.Name, StringComparer.CurrentCulture))
            {
                string imageKey = $"[{sheet.Family}] {sheet.Name}";
                images.Images.Add(imageKey, sheet.CreateThumbnail());

                string caption = sheet is FixedSheet fixedSheet
                    ? $"{translator[sheet.Name]}{Environment.NewLine}[{fixedSheet.DefaultWidthInMM} x {fixedSheet.DefaultHeightInMM}]"
                    : translator[sheet.Name];

                lstView.Items.Add(new ListViewItem(caption, imageKey) { Tag = sheet.GetType(), Group = group });
            }
        }
    }

    private void btnOk_Click(object? sender, EventArgs e)
    {
        if (lstView.SelectedItems.Count != 1 || lstView.SelectedItems[0].Tag is not Type sheetType)
            return;

        Enabled = false;
        Core.Instance.AddDocument(new Document((Sheet)Activator.CreateInstance(sheetType)!) { Name = Core.Instance.CreateUntitledName() });
        Command.GetInstance<ActualSizeCommand>().Run();
        Close();
    }

    private void btnCancel_Click(object? sender, EventArgs e) => Close();

    private void lstView_SelectedIndexChanged(object? sender, EventArgs e) =>
        btnOk.Enabled = lstView.SelectedItems.Count == 1;

    private void lstView_DoubleClick(object? sender, EventArgs e)
    {
        if (lstView.SelectedItems.Count == 1)
            btnOk_Click(sender, e);
    }

    private void cbShowOnStartup_CheckStateChanged(object? sender, EventArgs e) =>
        Settings.Environment.ShowNewSheetDialogOnStartup = cbShowOnStartup.Checked;

    private void SheetsForm_Shown(object? sender, EventArgs e)
    {
        if (Core.Instance.IsFirstRun)
            Core.Instance.SetDefaultWindowsPos();
    }
}

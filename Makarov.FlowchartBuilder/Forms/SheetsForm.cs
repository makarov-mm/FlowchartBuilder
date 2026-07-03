using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Makarov.FlowchartBuilder.Commands;
using Makarov.FlowchartBuilder.Sheets;
using BaseSettings = Makarov.Framework.Core.Settings;

namespace Makarov.FlowchartBuilder.Forms
{
    public partial class SheetsForm : Form
    {

        public SheetsForm()
        {

            InitializeComponent();

            btnOk.Text = Core.Instance.CurrentTranslator["OK"];
            btnCancel.Text = Core.Instance.CurrentTranslator["Cancel"];

            string showOnStartup = Core.Instance.CachedSettingsStorage[Settings.RegistryKeys.NewSheetDialog.ShowOnStartup];
            cbShowOnStartup.Checked = string.IsNullOrEmpty(showOnStartup) || bool.Parse(showOnStartup);

            lstView.LargeImageList = new ImageList
            {
                ImageSize = new Size(Sheet.ThumbnailWidth, Sheet.ThumbnailHeight),
                ColorDepth = ColorDepth.Depth32Bit
            };
            lstView.ForeColor = Settings.Colors.ThumbnailText;

            var list = new List<KeyValuePair<string, List<KeyValuePair<string, Sheet>>>>();
            var asm = Assembly.GetExecutingAssembly();

            foreach (var cls in asm.GetTypes())

                if (cls.IsClass && cls.IsSubclassOf(typeof(Sheet)) && !cls.IsAbstract)
                {
                    var obj = (Sheet)Activator.CreateInstance(cls);
                    string family = obj.Family;
                    string name = obj.Name;

                    bool familyExists = false;
                    foreach (var kvp in list)
                        if (kvp.Key == family)
                        {
                            familyExists = true;
                            break;
                        }

                    if (!familyExists)
                    {
                        var kvp = new KeyValuePair<string, List<KeyValuePair<string, Sheet>>>(
                            family,
                            new List<KeyValuePair<string, Sheet>>());
                        list.Add(kvp);
                    }

                    foreach (var kvp in list)
                        if (kvp.Key == family)
                            kvp.Value.Add(new KeyValuePair<string, Sheet>(name, obj));
                }

            var sortedList = new List<KeyValuePair<string, List<KeyValuePair<string, Sheet>>>>();
            while (list.Count > 0)
            {
                var curr = list[0];

                foreach (var kvp in list)
                    if (kvp.Key.CompareTo(curr.Key) < 0)
                        curr = kvp;

                sortedList.Add(curr);
                list.Remove(curr);
            }

            foreach (var kvp in sortedList)
            {
                var sortedSheetsList = new List<KeyValuePair<string, Sheet>>();

                while (kvp.Value.Count > 0)
                {
                    var curr = kvp.Value[0];

                    foreach (var kvpSh in kvp.Value)
                        if (kvpSh.Key.CompareTo(curr.Key) < 0)
                            curr = kvpSh;

                    sortedSheetsList.Add(curr);
                    kvp.Value.Remove(curr);
                }

                foreach (var kvpSh in sortedSheetsList)
                    kvp.Value.Add(kvpSh);
            }

            var groups = new List<string>();
            foreach (var kvp in sortedList)
            {
                if (!groups.Contains(kvp.Key))
                {
                    groups.Add(kvp.Key);
                    lstView.Groups.Add(kvp.Key, Core.Instance.CurrentTranslator[kvp.Key]);
                }

                foreach (var kvps in kvp.Value)
                {
                    string name = kvps.Key;
                    Sheet obj = kvps.Value;

                    string id = string.Format("[{0}] {1}", obj.Family, obj.Name);
                    lstView.LargeImageList.Images.Add(id, obj.Thumbnail);

                    string caption = Core.Instance.CurrentTranslator[name];

                    if (obj is FixedSheet)
                    {
                        var fixedSheet = (FixedSheet) obj;
                        caption += Environment.NewLine + string.Format("[{0} x {1}]",
                                                                       fixedSheet.DefaultWidthInMM,
                                                                       fixedSheet.DefaultHeightInMM);
                    }

                    var item = new ListViewItem(caption, id)
                                   {
                                       Tag = obj.GetType(),
                                       Group = lstView.Groups[kvp.Key]
                                   };

                    lstView.Items.Add(item);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        public ListView SheetTypesListView
        {
            get { return lstView; }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {

            Enabled = false;

            if (lstView.SelectedItems.Count != 1)
            {
                Enabled = true;
                return;
            }

            var item = lstView.SelectedItems[0];
            Core.Instance.AddDocument(new Document((Sheet)Activator.CreateInstance((Type)item.Tag)));
            Command.GetInstance("ActualSizeCommand").Run();
            Core.Instance.Redraw();

            Close();
        }

        private void lstView_SelectedIndexChanged(object sender, EventArgs e)
        {

            btnOk.Enabled = lstView.SelectedItems.Count == 1;
        }

        private void cbShowOnStartup_CheckStateChanged(object sender, EventArgs e)
        {

            Core.Instance.CachedSettingsStorage[Settings.RegistryKeys.NewSheetDialog.ShowOnStartup] =
                cbShowOnStartup.Checked.ToString();
        }

        private void FBNew_FormClosing(object sender, FormClosingEventArgs e)
        {

            Core.Instance.CachedSettingsStorage[Settings.RegistryKeys.NewSheetDialog.ShowOnStartup] =
                cbShowOnStartup.Checked.ToString();
        }

        private void lstView_DoubleClick(object sender, EventArgs e)
        {

            if (lstView.SelectedItems.Count == 1)
                btnOk_Click(sender, e);
        }

        private void SheetsForm_Shown(object sender, EventArgs e)
        {

            if (Core.Instance.IsFirstRun)
                Core.Instance.SetDefaultWindowsPos();
        }
    }
}

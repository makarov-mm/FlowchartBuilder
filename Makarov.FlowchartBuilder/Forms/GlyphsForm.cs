using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.FlowchartBuilder.Commands;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Forms
{
    public partial class GlyphsForm : DockingForm
    {
        public GlyphsForm()
        {
            InitializeComponent();
        }

        protected override string RegistrySettingsName
        {
            get { return "GlyphsForm"; }
        }

        private void FBGlyphs_Resize(object sender, EventArgs e)
        {
            lstView.Left = ClientRectangle.Left;
            lstView.Top = ClientRectangle.Top;
            lstView.Width = ClientRectangle.Width;
            lstView.Height = ClientRectangle.Height;
        }

        private void FBGlyphs_Shown(object sender, EventArgs e)
        {
            FBGlyphs_Resize(sender, new EventArgs());

            lstView.LargeImageList = new ImageList { ColorDepth = ColorDepth.Depth32Bit };

            lstView.ForeColor = Settings.Colors.ThumbnailText;

            var groups = new List<KeyValuePair<string, KeyValuePair<List<Type>, List<Type>>>>();

            foreach (var glyphType in PluginsManager.Instance.VisibleGlyphs)
            {

                var glyph = (AbstractGlyph)Activator.CreateInstance(glyphType);

                if (!groups.Any(x => x.Key == glyph.Family))
                {
                    groups.Add(new KeyValuePair<string, KeyValuePair<List<Type>, List<Type>>>(
                        glyph.Family,
                        new KeyValuePair<List<Type>, List<Type>>(new List<Type>(), new List<Type>())));
                }

                if (glyph.OrderExists)
                {
                    bool added = false;

                    for (int i = 0; i < groups.First(x => x.Key == glyph.Family).Value.Value.Count; i++)
                    {

                        var tg = (AbstractGlyph)Activator.CreateInstance(groups.First(x => x.Key == glyph.Family).Value.Value[i]);

                        if (glyph.Order <= tg.Order)
                        {
                            groups.First(x => x.Key == glyph.Family).Value.Value.Insert(i, glyphType);
                            added = true;
                            break;
                        }
                    }

                    if (!added)
                        groups.First(x => x.Key == glyph.Family).Value.Value.Add(glyphType);
                }
                else
                {
                    groups.First(x => x.Key == glyph.Family).Value.Value.Add(glyphType);
                }
            }

            foreach (var group in groups.OrderBy(x => x.Key))
            {
                lstView.Groups.Add(group.Key, group.Key);

                foreach (var glyphType in group.Value.Key)
                {
                    var glyph = (AbstractGlyph)Activator.CreateInstance(glyphType);

                    if (glyph.ThumbnailWidth > lstView.LargeImageList.ImageSize.Width)
                        lstView.LargeImageList.ImageSize = new Size(
                            glyph.ThumbnailWidth,
                            lstView.LargeImageList.ImageSize.Height);

                    if (glyph.ThumbnailHeight > lstView.LargeImageList.ImageSize.Height)
                        lstView.LargeImageList.ImageSize = new Size(
                            lstView.LargeImageList.ImageSize.Width,
                            glyph.ThumbnailHeight);

                    lstView.LargeImageList.Images.Add(glyph.Name, glyph.Thumbnail);
                    var item = new ListViewItem(glyph.Name, glyph.Name)
                    {
                        Tag = glyphType,
                        Group = lstView.Groups[glyph.Family]
                    };
                    lstView.Items.Add(item);
                }

                foreach (var glyphType in group.Value.Value)
                {
                    var glyph = (AbstractGlyph) Activator.CreateInstance(glyphType);

                    if (glyph.ThumbnailWidth > lstView.LargeImageList.ImageSize.Width)
                        lstView.LargeImageList.ImageSize = new Size(
                            glyph.ThumbnailWidth,
                            lstView.LargeImageList.ImageSize.Height);

                    if (glyph.ThumbnailHeight > lstView.LargeImageList.ImageSize.Height)
                        lstView.LargeImageList.ImageSize = new Size(
                            lstView.LargeImageList.ImageSize.Width,
                            glyph.ThumbnailHeight);

                    string imageKey = glyph.Family + "::" + glyph.Name;
                    lstView.LargeImageList.Images.Add(imageKey, glyph.Thumbnail);
                    var item = new ListViewItem(glyph.Name, imageKey)
                                   {
                                       Tag = glyphType,
                                       Group = lstView.Groups[glyph.Family]
                                   };
                    lstView.Items.Add(item);
                }
            }
        }

        private void FBGlyphs_Load(object sender, EventArgs e)
        {

            Text = Core.Instance.CurrentTranslator["Wnd_Palette_Title"];
        }

        private void lstView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {

            if (!e.IsSelected)
                return;

            var glyphType = (Type)e.Item.Tag;

            var cmdAttrs = glyphType.GetCustomAttributes(typeof (CommandGlyphAttribute), true);
            bool isCmdGlyph = cmdAttrs.Length > 0;

            if (isCmdGlyph)
            {
                foreach (var cmdAttr in cmdAttrs)
                {

                    string cmd = ((CommandGlyphAttribute)cmdAttr).CommandName;

                    var command = Command.GetInstance(cmd);

                    if (command is NoneCommand)
                    {
                        ClearGlyphsSelection();
                        break;
                    }

                    var glyphCommand = (CommandedGlyphCommand)Command.GetInstance(cmd);

                    glyphCommand.GlyphType = glyphType;

                    glyphCommand.Run();
                }

                Core.Instance.CurrentGlyphType = null;
            }
            else
            {
                Core.Instance.CurrentGlyphType = glyphType;
            }

            Core.Instance.Redraw();
        }

        private void lstView_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            if (!Core.Instance.CurrGlyphTypeSelected)
                return;

            Core.Instance.CurrentDocument.DocumentSheet.DeselectAllGlyphs();
            var glyph = (AbstractGlyph) Activator.CreateInstance(Core.Instance.CurrentGlyphType);
            glyph.Selected = true;
            Core.Instance.CurrentDocument.DocumentSheet.Add(glyph);
            Core.Instance.CurrentGlyphType = null;

            Core.Instance.Redraw();
        }

        private void FBGlyphs_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Core.Instance.MainWindow.IsClosing)
                return;

            e.Cancel = true;
            Hide();

            Command.GetInstance("PaletteWindowCommand").Enabled = true;
        }

        public void ClearGlyphsSelection()
        {

            lstView.SelectedIndices.Clear();
        }
    }
}

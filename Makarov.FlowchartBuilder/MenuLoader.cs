using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using Makarov.FlowchartBuilder.Commands;
using Makarov.FlowchartBuilder.Forms;
using Makarov.Framework.Components;

namespace Makarov.FlowchartBuilder
{

    public static class MenuLoader
    {
        #region Public methods

        public static void LoadMainMenu(MainForm wnd)
        {

            var doc = new XmlDocument();
            doc.Load(Settings.Files.MainMenuDefinition);
            doc.Schemas.Add(Settings.Xml.Namespaces.Base, Settings.Files.MainMenuSchema);
            doc.Validate(delegate(object sender, ValidationEventArgs args) { throw args.Exception; });

            var mainNode = doc.GetElementsByTagName(Settings.Xml.Tags.MainMenu)[0];

            foreach (XmlNode node in mainNode)
            {

                if (node.NodeType == XmlNodeType.Element && node.Name == Settings.Xml.Tags.Menu)
                {
                    wnd.MainMenu.Items.Add(CreateSubmenu(node));
                }

                else if (node.NodeType != XmlNodeType.Comment)
                    throw new InvalidNodeTypeException(node.NodeType.ToString());
            }
        }

        public static void LoadSecondaryMenu(MainForm wnd)
        {

            var doc = new XmlDocument();
            doc.Load(Settings.Files.SecondaryMenuDefinition);
            doc.Schemas.Add(Settings.Xml.Namespaces.Base, Settings.Files.SecondaryMenuSchema);
            doc.Validate(delegate(object sender, ValidationEventArgs args) { throw args.Exception; });

            var mainNode = doc.GetElementsByTagName(Settings.Xml.Tags.SecondaryMenu)[0];

            foreach (XmlNode node in mainNode)
            {

                if (node.NodeType == XmlNodeType.Element)
                {
                    switch (node.Name)
                    {

                        case Settings.Xml.Tags.Command:
                            wnd.SecondaryMenu.Items.Add(CreateButton(node));
                            break;

                        case Settings.Xml.Tags.Separator:
                            wnd.SecondaryMenu.Items.Add(CreateSeparator());
                            break;

                        default:
                            throw new InvalidNodeNameException(node.Name);
                    }
                }

                else if (node.NodeType != XmlNodeType.Comment)
                    throw new InvalidNodeTypeException(node.NodeType.ToString());
            }
        }
        #endregion

        #region Private methods

        private static ToolStripItem CreateSubmenu(XmlNode node)
        {
            var item = new ToolStripMenuItem(
                Core.Instance.CurrentTranslator[node.Attributes[Settings.Xml.Attributes.Caption].Value]);

            if (node.Attributes[Settings.Xml.Attributes.Icon] != null)
            {
                string icon = node.Attributes[Settings.Xml.Attributes.Icon].Value;
                if (!string.IsNullOrEmpty(icon))
                {
                    item.ImageScaling = ToolStripItemImageScaling.None;
                    item.ImageTransparentColor = Color.White;
                    item.Image = Core.Instance.Icons[icon];
                }
            }

            foreach (XmlNode subNode in node)
            {
                if (subNode.NodeType == XmlNodeType.Element)
                {
                    switch (subNode.Name)
                    {

                        case Settings.Xml.Tags.Menu:
                            item.DropDownItems.Add(CreateSubmenu(subNode));
                            break;

                        case Settings.Xml.Tags.Command:
                            item.DropDownItems.Add(CreateCommand(subNode));
                            break;

                        case Settings.Xml.Tags.Separator:
                            item.DropDownItems.Add(CreateSeparator());
                            break;

                        default:
                            throw new InvalidNodeNameException(node.Name);
                    }
                }
                else if (subNode.NodeType != XmlNodeType.Comment)
                    throw new InvalidNodeTypeException(subNode.NodeType.ToString());
            }

            return item;
        }

        private static ToolStripItem CreateCommand(XmlNode node)
        {

            var cmdValue = node.Attributes[Settings.Xml.Attributes.Class].Value;

            if (cmdValue[0] == '@')
            {
                cmdValue = cmdValue.Substring(1);

                var submenu = new ToolStripMenuItem(
                Core.Instance.CurrentTranslator[node.Attributes[Settings.Xml.Attributes.Caption].Value]);

                if (node.Attributes[Settings.Xml.Attributes.Icon] != null)
                {
                    string icon = node.Attributes[Settings.Xml.Attributes.Icon].Value;
                    if (!string.IsNullOrEmpty(icon))
                    {
                        submenu.ImageScaling = ToolStripItemImageScaling.None;
                        submenu.ImageTransparentColor = Color.White;
                        submenu.Image = Core.Instance.Icons[icon];
                    }
                }

                foreach (var cmd in ((CommandWithSubcommands)Command.GetInstance(cmdValue)).Commands)
                {
                    var subitem = new ActiveToolStripMenuItem(
                        Core.Instance.CurrentTranslator[cmd],
                        string.Format("{0}@{1}", cmdValue, cmd));

                    subitem.Click += (e, args) =>
                                         {
                                             var commandName = ((string) subitem.Tag).Split('@')[0];
                                             var commandParam = ((string) subitem.Tag).Split('@')[1];
                                             var instance = (CommandWithSubcommands) Command.GetInstance(commandName);
                                             instance.Run(commandParam);
                                         };

                    subitem.ImageScaling = ToolStripItemImageScaling.None;
                    subitem.ImageTransparentColor = Color.White;
                    subitem.ActiveIcon = ((CommandWithSubcommands)Command.GetInstance(cmdValue)).Icon(cmd);

                    submenu.DropDownItems.Add(subitem);
                }

                return submenu;
            }

            var item = new ActiveToolStripMenuItem(
                node.Attributes[Settings.Xml.Attributes.Caption] != null
                    ? Core.Instance.CurrentTranslator[node.Attributes[Settings.Xml.Attributes.Caption].Value]
                    : string.Empty,
                cmdValue);

            if (node.Attributes[Settings.Xml.Attributes.Icon] != null)
            {
                string icon = node.Attributes[Settings.Xml.Attributes.Icon].Value;
                if (!string.IsNullOrEmpty(icon))
                {
                    item.ImageScaling = ToolStripItemImageScaling.None;
                    item.ImageTransparentColor = Color.White;
                    item.ActiveIcon = Core.Instance.Icons[icon];
                }
            }

            if (node.Attributes[Settings.Xml.Attributes.Shortcut] != null)
            {
                string shortcut = node.Attributes[Settings.Xml.Attributes.Shortcut].Value;
                if (!string.IsNullOrEmpty(shortcut))
                {
                    string[] parts = shortcut.Split('+');
                    Keys keys = 0;

                    foreach (string part in parts)
                    {
                        switch (part.ToLowerInvariant())
                        {
                            case "ctrl":
                                keys |= Keys.Control;
                                break;

                            case "alt":
                                keys |= Keys.Alt;
                                break;

                            case "shift":
                                keys |= Keys.Shift;
                                break;

                            default:
                                Keys parsed;
                                if (Enum.TryParse(part, out parsed))
                                    keys |= parsed;
                                else
                                    throw new FlowchartBuilderException(
                                        string.Format(@"Key '{0}' not parsed.", part));
                                break;
                        }
                    }

                    item.ShortcutKeys = keys;
                }
            }

            var checkedAttrib = node.Attributes[Settings.Xml.Attributes.Checked];
            item.ActiveChecked = checkedAttrib != null
                                     ? bool.Parse(checkedAttrib.Value)
                                     : false;

            item.ActiveBalloon = node.Attributes[Settings.Xml.Attributes.ToolTip] != null
                                    ? Core.Instance.CurrentTranslator[node.Attributes[Settings.Xml.Attributes.ToolTip].Value]
                                    : string.Empty;

            if (node.Attributes[Settings.Xml.Attributes.Enabled] != null)
                item.ActiveEnabled = bool.Parse(node.Attributes[Settings.Xml.Attributes.Enabled].Value);

            Command.GetInstance(cmdValue, item);

            item.Click += (e, args) => Command.GetInstance((string) item.Tag).Run();

            return item;
        }

        private static ToolStripItem CreateSeparator()
        {
            return new ToolStripSeparator();
        }

        private static ToolStripItem CreateButton(XmlNode node)
        {

            var item = new ActiveToolStripButton(
                node.Attributes[Settings.Xml.Attributes.Caption] != null
                    ? Core.Instance.CurrentTranslator[node.Attributes[Settings.Xml.Attributes.Caption].Value]
                    : string.Empty,
                node.Attributes[Settings.Xml.Attributes.Class].Value);

            if (node.Attributes[Settings.Xml.Attributes.Icon] != null)
            {
                string icon = node.Attributes[Settings.Xml.Attributes.Icon].Value;
                if (!string.IsNullOrEmpty(icon))
                {
                    item.ImageScaling = ToolStripItemImageScaling.None;
                    item.ImageTransparentColor = Color.White;
                    item.ActiveIcon = Core.Instance.Icons[icon];
                }
            }

            var checkedAttrib = node.Attributes[Settings.Xml.Attributes.Checked];
            item.ActiveChecked = checkedAttrib != null
                                     ? bool.Parse(checkedAttrib.Value)
                                     : false;

            item.ActiveBalloon = node.Attributes[Settings.Xml.Attributes.ToolTip] != null
                                    ? Core.Instance.CurrentTranslator[node.Attributes[Settings.Xml.Attributes.ToolTip].Value]
                                    : string.Empty;

            if (node.Attributes[Settings.Xml.Attributes.Enabled] != null)
                item.ActiveEnabled = bool.Parse(node.Attributes[Settings.Xml.Attributes.Enabled].Value);

            Command.GetInstance((string) item.Tag, item);

            item.Click += (e, args) => Command.GetInstance((string) item.Tag).Run();

            return item;
        }
        #endregion
    }
}

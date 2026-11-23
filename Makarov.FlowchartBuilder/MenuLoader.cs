using System.Xml;
using Makarov.FlowchartBuilder.Commands;
using Makarov.FlowchartBuilder.Forms;
using Makarov.Framework.Core.Xml;

namespace Makarov.FlowchartBuilder;

/// <summary>
/// Builds the main menu and the toolbar from Data\MainMenu.xml and Data\SecondaryMenu.xml.
/// Captions and tooltips in the files are translation keys; command classes are bound by name.
/// </summary>
public static class MenuLoader
{
    private const char SubcommandsPrefix = '@';

    public static void LoadMainMenu(MainForm window)
    {
        XmlElement root = ValidatedXml.Load(
            Settings.Files.MainMenuDefinition, Settings.Xml.Namespace, Settings.Files.MainMenuSchema);

        foreach (XmlElement node in root.ChildNodes.OfType<XmlElement>())
            window.MainMenu.Items.Add(CreateMenuItem(node));

        AddDocumentTools(window);
    }

    private static void AddDocumentTools(MainForm window)
    {
        var core = Core.Instance;
        string language = core.CurrentTranslator.Language;
        var menu = new ToolStripMenuItem(core.CurrentTranslator["Menu_Plugins"]);
        var examples = new ToolStripMenuItem(core.CurrentTranslator["Menu_Examples"]);
        foreach (var type in core.Plugins.Types.Where(t => t.IsClass && !t.IsAbstract && typeof(IDocumentTool).IsAssignableFrom(t))
                     .OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            var tool = (IDocumentTool)Activator.CreateInstance(type)!;
            var item = new ToolStripMenuItem(tool.Title(language));
            item.Click += (_, _) =>
            {
                if (!core.HasDocuments) return;
                try { tool.Run(core.CurrentDocument, window, language); }
                catch (Exception ex) { core.ShowError(ex.Message); }
            };
            menu.DropDownItems.Add(item);
            if (tool.ExampleFileName is { } name)
            {
                var file = Path.Combine(AppContext.BaseDirectory, "Examples", Path.GetFileName(name));
                if (File.Exists(file))
                {
                    string label = Path.GetFileNameWithoutExtension(file);
                    examples.DropDownItems.Add(label, null, (_, _) =>
                    {
                        if (core.TryOpenExample(file) && tool.ExampleInstructions(language) is { } instructions)
                            MessageBox.Show(window, instructions, label, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                }
            }
        }
        if (examples.DropDownItems.Count > 0) menu.DropDownItems.Add(examples); else examples.Dispose();
        if (menu.DropDownItems.Count == 0) { menu.Dispose(); return; }
        menu.DropDownOpening += (_, _) =>
        {
            foreach (ToolStripItem item in menu.DropDownItems) item.Enabled = item == examples || core.HasDocuments;
        };
        window.MainMenu.Items.Add(menu);
    }

    public static void LoadSecondaryMenu(MainForm window)
    {
        XmlElement root = ValidatedXml.Load(
            Settings.Files.SecondaryMenuDefinition, Settings.Xml.Namespace, Settings.Files.SecondaryMenuSchema);

        foreach (XmlElement node in root.ChildNodes.OfType<XmlElement>())
        {
            window.SecondaryMenu.Items.Add(node.Name switch
            {
                Settings.Xml.Tags.Command => CreateButton(node),
                Settings.Xml.Tags.Separator => new ToolStripSeparator(),
                _ => throw UnexpectedElement(node),
            });
        }
    }

    private static ToolStripItem CreateMenuItem(XmlElement node) => node.Name switch
    {
        Settings.Xml.Tags.Menu => CreateSubmenu(node),
        Settings.Xml.Tags.Command => CreateCommandItem(node),
        Settings.Xml.Tags.Separator => new ToolStripSeparator(),
        _ => throw UnexpectedElement(node),
    };

    private static ToolStripMenuItem CreateSubmenu(XmlElement node)
    {
        var menu = new ToolStripMenuItem(Translate(node, Settings.Xml.Attributes.Caption));
        SetIcon(menu, node);

        foreach (XmlElement child in node.ChildNodes.OfType<XmlElement>())
            menu.DropDownItems.Add(CreateMenuItem(child));

        return menu;
    }

    private static ToolStripMenuItem CreateCommandItem(XmlElement node)
    {
        string commandName = node.GetAttribute(Settings.Xml.Attributes.Class);
        if (commandName.StartsWith(SubcommandsPrefix))
            return CreateSubcommandsMenu(node, (CommandWithSubcommands)Command.GetInstance(commandName[1..]));

        var item = new ToolStripMenuItem(Translate(node, Settings.Xml.Attributes.Caption))
        {
            ShortcutKeys = ParseShortcut(node.GetAttribute(Settings.Xml.Attributes.Shortcut)),
        };

        BindCommand(item, node, Command.GetInstance(commandName));
        return item;
    }

    private static ToolStripMenuItem CreateSubcommandsMenu(XmlElement node, CommandWithSubcommands command)
    {
        var menu = new ToolStripMenuItem(Translate(node, Settings.Xml.Attributes.Caption));
        SetIcon(menu, node);

        foreach (string subcommand in command.Commands)
        {
            var item = new ToolStripMenuItem(Core.Instance.CurrentTranslator[subcommand])
            {
                ImageScaling = ToolStripItemImageScaling.None,
                ImageTransparentColor = Color.White,
                Image = command.GetIcon(subcommand),
            };

            item.Click += (_, _) => command.Execute(subcommand);
            menu.DropDownItems.Add(item);
        }

        return menu;
    }

    private static ToolStripButton CreateButton(XmlElement node)
    {
        var button = new ToolStripButton(Translate(node, Settings.Xml.Attributes.Caption));
        BindCommand(button, node, Command.GetInstance(node.GetAttribute(Settings.Xml.Attributes.Class)));
        return button;
    }

    private static void BindCommand(ToolStripItem item, XmlElement node, Command command)
    {
        SetIcon(item, node);
        item.ToolTipText = Translate(node, Settings.Xml.Attributes.ToolTip);

        if (node.HasAttribute(Settings.Xml.Attributes.Enabled))
            item.Enabled = bool.Parse(node.GetAttribute(Settings.Xml.Attributes.Enabled));

        command.Attach(item);

        // On/off options show their saved state; the Checked attribute is only a default for other commands.
        command.Checked = command is ToggleCommand toggle
            ? toggle.IsOn
            : node.HasAttribute(Settings.Xml.Attributes.Checked) && bool.Parse(node.GetAttribute(Settings.Xml.Attributes.Checked));

        item.Click += (_, _) => command.Execute();
    }

    private static void SetIcon(ToolStripItem item, XmlElement node)
    {
        string icon = node.GetAttribute(Settings.Xml.Attributes.Icon);
        if (icon.Length == 0)
            return;

        item.ImageScaling = ToolStripItemImageScaling.None;
        item.ImageTransparentColor = Color.White;
        item.Image = Core.Instance.Icons.GetCopy(icon);
    }

    /// <summary>Translates the attribute value; a missing attribute gives an empty string.</summary>
    private static string Translate(XmlElement node, string attribute)
    {
        string key = node.GetAttribute(attribute);
        return key.Length == 0 ? string.Empty : Core.Instance.CurrentTranslator[key];
    }

    /// <summary>Parses shortcuts such as "Ctrl+Shift+S", "Delete" or "F1".</summary>
    private static Keys ParseShortcut(string shortcut)
    {
        if (shortcut.Length == 0)
            return Keys.None;

        Keys keys = Keys.None;

        foreach (string part in shortcut.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            keys |= part.ToLowerInvariant() switch
            {
                "ctrl" => Keys.Control,
                "alt" => Keys.Alt,
                "shift" => Keys.Shift,
                _ when Enum.TryParse(part, ignoreCase: true, out Keys key) => key,
                _ => throw new InvalidDataException($"Unknown key '{part}' in shortcut '{shortcut}'."),
            };
        }

        return keys;
    }

    private static InvalidDataException UnexpectedElement(XmlElement node) =>
        new($"Unexpected element '{node.Name}' in a menu definition.");
}

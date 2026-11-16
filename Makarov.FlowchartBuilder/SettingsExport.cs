using System.Xml;
using Makarov.Framework.Core;

namespace Makarov.FlowchartBuilder;

/// <summary>
/// "Settings / Manage settings": copies all user settings to or from a *.fbsettings XML file.
/// </summary>
public static class SettingsExport
{
    private const string RootTag = "Settings";
    private const string EntryTag = "Key";
    private const string NameAttribute = "name";

    private static string Filter => $"{Core.Instance.CurrentTranslator["SettingsFiles"]} (*.{Settings.Environment.SettingsExtension})|*.{Settings.Environment.SettingsExtension}";

    public static void ShowSaveDialog()
    {
        using var dialog = new SaveFileDialog
        {
            DefaultExt = Settings.Environment.SettingsExtension,
            Filter = Filter,
            FileName = "FlowchartBuilder." + Settings.Environment.SettingsExtension,
        };

        if (dialog.ShowDialog(Core.Instance.MainWindow) == DialogResult.OK)
            Run(() => Save(dialog.FileName));
    }

    public static void ShowLoadDialog()
    {
        using var dialog = new OpenFileDialog
        {
            DefaultExt = Settings.Environment.SettingsExtension,
            Filter = Filter,
        };

        if (dialog.ShowDialog(Core.Instance.MainWindow) != DialogResult.OK || !Run(() => Load(dialog.FileName)))
            return;

        Translator translator = Core.Instance.CurrentTranslator;
        MessageBox.Show(
            Core.Instance.MainWindow,
            translator["Msg_Need_Restart_Application"],
            translator["Msg_Info"],
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    public static void Save(string fileName)
    {
        var storage = Core.Instance.SettingsStorage;
        var document = new XmlDocument();
        XmlElement root = document.CreateElement(RootTag);
        document.AppendChild(root);

        foreach (string key in storage.Keys.Order(StringComparer.Ordinal))
        {
            XmlElement entry = document.CreateElement(EntryTag);
            entry.SetAttribute(NameAttribute, key);
            entry.InnerText = storage[key] ?? string.Empty;
            root.AppendChild(entry);
        }

        document.Save(fileName);
    }

    public static void Load(string fileName)
    {
        var document = new XmlDocument();
        document.Load(fileName);

        if (document.DocumentElement is not { Name: RootTag } root)
            throw new InvalidDataException($"'{fileName}' is not a FlowchartBuilder settings file.");

        var storage = Core.Instance.SettingsStorage;
        foreach (XmlElement entry in root.ChildNodes.OfType<XmlElement>().Where(entry => entry.Name == EntryTag))
        {
            string key = entry.GetAttribute(NameAttribute);
            if (key.Length > 0)
                storage[key] = entry.InnerText;
        }
    }

    private static bool Run(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or XmlException or InvalidDataException)
        {
            Core.Instance.ShowError(e.Message);
            return false;
        }
    }
}

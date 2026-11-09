using System.Xml;

namespace Makarov.Framework.Core.SettingStorages;

/// <summary>
/// Stores settings in an XML file under %APPDATA%; used when the registry is not accessible.
/// </summary>
public sealed class XmlStorage : ISettingsStorage
{
    private const string RootTag = "Settings";
    private const string EntryTag = "Key";
    private const string NameAttribute = "name";

    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

    public XmlStorage(string fileName)
    {
        FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), fileName);
        Load();
    }

    public string FileName { get; }

    public string? this[string key]
    {
        get => _values.GetValueOrDefault(key);
        set
        {
            if (value is null)
            {
                if (!_values.Remove(key))
                    return;
            }
            else
            {
                if (_values.TryGetValue(key, out string? old) && old == value)
                    return;

                _values[key] = value;
            }

            Save();
        }
    }

    public IEnumerable<string> Keys => _values.Keys;

    public bool Contains(string key) => _values.ContainsKey(key);

    private void Load()
    {
        if (!File.Exists(FileName))
            return;

        var document = new XmlDocument();
        document.Load(FileName);

        if (document.DocumentElement is null)
            return;

        foreach (XmlElement entry in document.DocumentElement.ChildNodes.OfType<XmlElement>())
        {
            string name = entry.GetAttribute(NameAttribute);
            if (name.Length > 0)
                _values[name] = entry.InnerText;
        }
    }

    private void Save()
    {
        var document = new XmlDocument();
        XmlElement root = document.CreateElement(RootTag);
        document.AppendChild(root);

        foreach ((string name, string value) in _values)
        {
            XmlElement entry = document.CreateElement(EntryTag);
            entry.SetAttribute(NameAttribute, name);
            entry.InnerText = value;
            root.AppendChild(entry);
        }

        document.Save(FileName);
    }
}

using System.Globalization;
using System.Xml;
using Makarov.Framework.Core.Xml;

namespace Makarov.FlowchartBuilder;

public sealed class UnitsManager
{
    private readonly Dictionary<string, Units> _units = new(StringComparer.Ordinal);

    public UnitsManager(string definitionFile, string schemaFile)
    {
        XmlElement root = ValidatedXml.Load(definitionFile, Settings.Xml.Namespace, schemaFile);

        foreach (XmlElement node in root.ChildNodes.OfType<XmlElement>())
        {
            Add(new Units(
                node.GetAttribute(Settings.Xml.Attributes.Name),
                node.GetAttribute(Settings.Xml.Attributes.ShortName),
                float.Parse(node.GetAttribute(Settings.Xml.Attributes.InMM), NumberStyles.Float, CultureInfo.InvariantCulture),
                int.Parse(node.GetAttribute(Settings.Xml.Attributes.BigStep), CultureInfo.InvariantCulture),
                int.Parse(node.GetAttribute(Settings.Xml.Attributes.SmallStep), CultureInfo.InvariantCulture)));
        }
    }

    public Units this[string name] =>
        _units.TryGetValue(name, out Units? units) ? units : throw new KeyNotFoundException($"Units '{name}' not found.");

    public IEnumerable<string> LoadedUnits => _units.Keys;

    public bool Contains(string name) => _units.ContainsKey(name);

    private void Add(Units units)
    {
        if (!_units.TryAdd(units.Name, units))
            throw new InvalidDataException($"Units '{units.Name}' are defined twice.");
    }
}

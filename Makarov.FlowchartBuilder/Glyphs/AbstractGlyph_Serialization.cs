using System.Xml;

namespace Makarov.FlowchartBuilder.Glyphs;

public abstract partial class AbstractGlyph
{
    /// <summary>
    /// Writes the glyph as <c>&lt;fb:Glyph fb:ClassName="..." id="..."&gt;</c> with one <c>fb:Property</c> per document property.
    /// </summary>
    public XmlElement Serialize(XmlDocument document)
    {
        XmlElement node = document.CreateElement(Settings.Xml.Tags.Glyph, Settings.Xml.Namespace);

        XmlAttribute className = document.CreateAttribute(Settings.Xml.Attributes.ClassName, Settings.Xml.Namespace);
        className.Value = GetType().FullName;
        node.Attributes.Append(className);
        node.SetAttribute(Settings.Xml.Attributes.Id, Id);

        if (Fixed)
            node.SetAttribute(Settings.Xml.Attributes.Fixed, bool.TrueString);

        foreach (var property in Metadata.ActiveProperties)
        {
            XmlElement propertyNode = document.CreateElement(Settings.Xml.Tags.Property, Settings.Xml.Namespace);

            XmlAttribute name = document.CreateAttribute(Settings.Xml.Attributes.Value, Settings.Xml.Namespace);
            name.Value = property.Name;
            propertyNode.Attributes.Append(name);
            propertyNode.InnerText = Core.Instance.Serializers.Serialize(property.PropertyType, property.GetValue(this));

            node.AppendChild(propertyNode);
        }

        return node;
    }

    /// <summary>Reads what <see cref="Serialize"/> wrote.</summary>
    /// <returns>Names of stored properties this glyph type no longer has (the document was saved by another version).</returns>
    public IReadOnlyList<string> Deserialize(XmlElement node)
    {
        string id = node.GetAttribute(Settings.Xml.Attributes.Id);
        if (id.Length > 0)
            Id = id;

        Fixed = bool.TryParse(node.GetAttribute(Settings.Xml.Attributes.Fixed), out bool isFixed) && isFixed;

        var unknownProperties = new List<string>();

        foreach (XmlElement propertyNode in node.ChildNodes.OfType<XmlElement>())
        {
            string name = propertyNode.GetAttribute(Settings.Xml.Attributes.Value);
            var property = Metadata.FindActiveProperty(name);

            if (property is null)
            {
                unknownProperties.Add(name);
                continue;
            }

            property.SetValue(this, Core.Instance.Serializers.Deserialize(property.PropertyType, propertyNode.InnerText));
        }

        return unknownProperties;
    }
}

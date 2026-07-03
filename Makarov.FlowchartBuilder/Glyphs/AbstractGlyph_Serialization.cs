using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Makarov.FlowchartBuilder.Box;

namespace Makarov.FlowchartBuilder.Glyphs
{

    public abstract partial class AbstractGlyph
    {

        public IEnumerable<KeyValuePair<string, string>> SerializeProperties()
        {

            foreach (PropertyInfo property in GlyphsHelper.GetActiveProperties(this))
            {
                yield return new KeyValuePair<string, string>(
                    property.Name,
                    Core.Instance.Serializers.Serialize(property.PropertyType, property.GetValue(this, null)));
            }

        }

        public IEnumerable<string> DeserializeProperties(IEnumerable<KeyValuePair<string, string>> pairs)
        {

            var activeProperties = new List<PropertyInfo>(GlyphsHelper.GetActiveProperties(this));

            foreach (KeyValuePair<string, string> kvp in pairs)
            {

                bool finded = false;

                foreach (PropertyInfo property in activeProperties)
                {

                    if (property.Name == kvp.Key)
                    {
                        property.SetValue(
                            this,
                            Core.Instance.Serializers.Deserialize(property.PropertyType, kvp.Value),
                            null);

                        finded = true;

                        break;
                    }
                }

                if (!finded)
                    yield return kvp.Key;
            }
        }

        public XmlNode Serialize(XmlDocument doc)
        {
            OnBeginSerialization(doc);

            var node = (XmlElement)doc.CreateNode(
                XmlNodeType.Element,
                Settings.Xml.Tags.Glyph,
                Settings.Xml.Namespaces.Base);

            XmlAttribute glyphClassAttrib = doc.CreateAttribute(
                Settings.Xml.Attributes.GlyphClassName,
                Settings.Xml.Namespaces.Base);
            glyphClassAttrib.Value = GetType().FullName;
            node.Attributes.Append(glyphClassAttrib);

            XmlAttribute glyphIdAttrib = doc.CreateAttribute("id");
            glyphIdAttrib.Value = Id;
            node.Attributes.Append(glyphIdAttrib);

            foreach (KeyValuePair<string, string> kvp in SerializeProperties())
            {

                var subNode = (XmlElement)doc.CreateNode(
                    XmlNodeType.Element,
                    Settings.Xml.Tags.Property,
                    Settings.Xml.Namespaces.Base);

                XmlAttribute attrib = doc.CreateAttribute(
                    Settings.Xml.Attributes.Value,
                    Settings.Xml.Namespaces.Base);
                attrib.Value = kvp.Key;
                subNode.Attributes.Append(attrib);

                subNode.InnerText = kvp.Value;

                node.AppendChild(subNode);
            }

            return OnEndSerialization(node);
        }

        protected virtual void OnBeginSerialization(XmlDocument doc) { }

        protected virtual XmlNode OnEndSerialization(XmlNode node)
        {
            return node;
        }

        public IEnumerable<string> Deserialize(XmlNode node)
        {
            node = OnBeginDeserialization(node);

            if (node.Attributes != null)
            {
                XmlAttribute idAttrib = node.Attributes["id"];

                if (idAttrib != null && !string.IsNullOrEmpty(idAttrib.Value))
                    Id = idAttrib.Value;
            }

            var properties = new Dictionary<string, string>();

            foreach (XmlNode subNode in node)
            {

                string propName = subNode.Attributes[Settings.Xml.Attributes.Value].Value;

                string propValue = subNode.InnerText;

                properties.Add(propName, propValue);
            }

            return DeserializeProperties(OnEndDeserialization(properties));
        }

        protected XmlNode OnBeginDeserialization(XmlNode node)
        {
            return node;
        }

        protected IEnumerable<KeyValuePair<string, string>> OnEndDeserialization(IEnumerable<KeyValuePair<string, string>> properties)
        {
            return properties;
        }
    }
}

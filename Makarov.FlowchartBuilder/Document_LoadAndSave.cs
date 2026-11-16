using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Sheets;

namespace Makarov.FlowchartBuilder
{

    public sealed partial class Document
    {

        public void Load(string fileName)
        {

            Name = Path.GetFileNameWithoutExtension(fileName);

            var doc = new XmlDocument();

            doc.Load(fileName);

            var mainNode = doc.DocumentElement;

            if (mainNode == null)
                throw new MainDocumentNodeNotFoundException();

            var docVersionAttrib = mainNode.GetAttribute(Settings.Xml.Attributes.ProgramVersion);
            var version = new Version(docVersionAttrib);

            XmlElement sheetNode = null;

            foreach (XmlNode node in mainNode.ChildNodes)
                if (node.Name == Settings.Xml.Tags.Sheet && node is XmlElement)
                {
                    sheetNode = (XmlElement)node;
                    break;
                }

            if (sheetNode == null)
                throw new SheetNodeNotFoundException();

            // Older files may describe a paper size. Glyph coordinates use the same space.
            DocumentSheet = new InfiniteSheet();
            if (TryReadViewValue("ViewX", out float x) && TryReadViewValue("ViewY", out float y) &&
                TryReadViewValue("Zoom", out float zoom) && zoom > 0)
            {
                DocumentSheet.ScaleFactor = zoom;
                DocumentSheet.ViewOrigin = new PointF(x, y);
            }

            bool TryReadViewValue(string name, out float value) =>
                float.TryParse(sheetNode.GetAttribute(name), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                && float.IsFinite(value);

            // Add inserts at the front. Read back-to-front to retain the saved stacking order.
            foreach (XmlElement glyphNode in sheetNode.ChildNodes.OfType<XmlElement>().Reverse())
            {

                var glyphTypeAttrib = glyphNode.Attributes[Settings.Xml.Attributes.GlyphClassName];

                var glyphType = Type.GetType(glyphTypeAttrib.Value);

                if (glyphType is null)
                {
                    bool found = false;

                    foreach (var kvp in Core.Instance.PluginsLoader.LoadedAssemblies)
                    {
                        Assembly asm = kvp.Value;

                        foreach (var asmType in asm.GetTypes())
                        {

                            if (asmType.FullName == glyphTypeAttrib.Value)
                            {
                                glyphType = asmType;
                                found = true;
                            }
                        }

                        if (found)
                            break;
                    }
                }

                if (glyphType is null)
                    throw new GlyphTypeNotFoundException(glyphTypeAttrib.Value);

                if (glyphType.IsSubclassOf(typeof(AbstractGlyph)))
                {
                    var glyph = (AbstractGlyph)Activator.CreateInstance(glyphType);
                    if (glyph.Deserialize(glyphNode).ToList().Count > 0)
                        MessageBox.Show(@"Document upgraded.");
                    DocumentSheet.Add(glyph);
                }
                else
                    throw new GlyphTypeNotFoundException(glyphTypeAttrib.Value);
            }

            foreach (AbstractGlyph glyph in DocumentSheet.Glyphs)
            {
                var connector = glyph as ConnectorGlyph;

                if (connector != null)
                    connector.Relink(DocumentSheet.Glyphs);
            }

            DocumentSheet.RemoveOrphanConnectors();

            var historyEvents = new List<HistoryEvent>();

            foreach (AbstractGlyph glyph in DocumentSheet.Glyphs)
            {
                var snapshot = new GlyphSnapshot(glyph);
                historyEvents.Add(new HistoryEventCreateGlyph(snapshot));
            }

            DocumentHistory = new History(historyEvents);
            SaveSnapshots(GetSnapshotsFromSheet());

        }

        public void Save(string fileName)
        {

            var doc = new XmlDocument();

            var mainNode = (XmlElement)doc.CreateNode(
                XmlNodeType.Element,
                Settings.Xml.Tags.FlowchartBuilderDocument,
                Settings.Xml.Namespaces.Base);

            var versionAttrib = doc.CreateAttribute(Settings.Xml.Attributes.ProgramVersion);
            versionAttrib.Value = Assembly.GetEntryAssembly().GetName().Version.ToString();
            mainNode.Attributes.Append(versionAttrib);

            var sheetNode = (XmlElement)doc.CreateNode(
                XmlNodeType.Element,
                Settings.Xml.Tags.Sheet,
                Settings.Xml.Namespaces.Base);

            var sheet = DocumentSheet;

            var sheetClassAttrib = doc.CreateAttribute(
                Settings.Xml.Attributes.SheetClassName,
                Settings.Xml.Namespaces.Base);
            sheetClassAttrib.Value = sheet.GetType().ToString();
            sheetNode.Attributes.Append(sheetClassAttrib);

            sheetNode.SetAttribute("ViewX", XmlConvert.ToString(sheet.ViewOrigin.X));
            sheetNode.SetAttribute("ViewY", XmlConvert.ToString(sheet.ViewOrigin.Y));
            sheetNode.SetAttribute("Zoom", XmlConvert.ToString(sheet.ScaleFactor));

            foreach (var glyph in sheet.Glyphs)
                sheetNode.AppendChild(glyph.Serialize(doc));

            mainNode.AppendChild(sheetNode);

            doc.AppendChild(mainNode);

            doc.Save(fileName);

            SaveSnapshots(GetSnapshotsFromSheet());
        }
    }
}

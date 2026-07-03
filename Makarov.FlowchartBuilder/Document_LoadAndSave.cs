using System;
using System.Collections.Generic;
using System.IO;
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

            var sheetClassAttrib = sheetNode.Attributes[Settings.Xml.Attributes.SheetClassName];

            var sheetWidthAttrib = sheetNode.Attributes[Settings.Xml.Attributes.SheetWidth];
            var sheetHeightAttrib = sheetNode.Attributes[Settings.Xml.Attributes.SheetHeight];

            var sheetType = Type.GetType(sheetClassAttrib.Value);

            DocumentSheet = null;

            if (sheetType.IsSubclassOf(typeof(Sheet)))
            {

                if (sheetWidthAttrib != null && sheetHeightAttrib != null)
                {
                    var fixedSheet = (FixedSheet)Activator.CreateInstance(sheetType);
                    fixedSheet.Width = float.Parse(sheetWidthAttrib.Value);
                    fixedSheet.Height = float.Parse(sheetHeightAttrib.Value);
                    DocumentSheet = fixedSheet;
                }
                else
                    DocumentSheet = (Sheet)Activator.CreateInstance(sheetType);
            }

            if (DocumentSheet == null)
                throw new SheetNotLoadedException();

            foreach (XmlNode glyphNode in sheetNode)
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

            if (sheet is FixedSheet fixedSheet)
            {
                var sheetWidthAttrib = doc.CreateAttribute(Settings.Xml.Attributes.SheetWidth);
                sheetWidthAttrib.Value = fixedSheet.Width.ToString();
                sheetNode.Attributes.Append(sheetWidthAttrib);

                var sheetHeightAttrib = doc.CreateAttribute(Settings.Xml.Attributes.SheetHeight);
                sheetHeightAttrib.Value = fixedSheet.Height.ToString();
                sheetNode.Attributes.Append(sheetHeightAttrib);
            }

            foreach (var glyph in sheet.Glyphs)
                sheetNode.AppendChild(glyph.Serialize(doc));

            mainNode.AppendChild(sheetNode);

            doc.AppendChild(mainNode);

            doc.Save(fileName);

            SaveSnapshots(GetSnapshotsFromSheet());
        }
    }
}

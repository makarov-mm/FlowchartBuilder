using System.Globalization;
using System.Xml;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Sheets;

namespace Makarov.FlowchartBuilder;

/// <summary>
/// File format (*.mfb):
/// <code>
/// &lt;fb:FlowchartBuilderDocument Version="x.y.z.w"&gt;
///   &lt;fb:Sheet fb:ClassName="sheet type" ViewX="x" ViewY="y" Zoom="factor"&gt; fb:Glyph elements, front-most first &lt;/fb:Sheet&gt;
///   &lt;fb:Groups&gt; &lt;fb:Group&gt; &lt;fb:Member id="glyph id" /&gt; ... &lt;/fb:Group&gt; &lt;/fb:Groups&gt;
/// &lt;/fb:FlowchartBuilderDocument&gt;
/// </code>
/// Old paper formats open on the infinite canvas. Groups are optional and ignored by older versions.
/// </summary>
public sealed partial class Document
{
    /// <exception cref="IOException">The file cannot be read.</exception>
    /// <exception cref="XmlException">The file is not well-formed XML.</exception>
    /// <exception cref="InvalidDataException">The file is not a valid document or uses glyph types that are not installed.</exception>
    public static Document Load(string fileName)
    {
        XmlDocument xml = new();
        xml.Load(fileName);

        Sheet sheet;
        bool skippedProperties;

        try
        {
            sheet = ReadSheet(xml, fileName, out skippedProperties);
        }
        catch (Exception e) when (e is FormatException or OverflowException or ArgumentException or KeyNotFoundException or System.Reflection.TargetInvocationException)
        {
            throw Invalid(fileName, e.Message, e);
        }

        return new Document(sheet)
        {
            Name = Path.GetFileNameWithoutExtension(fileName),
            FileName = fileName,
            HasSkippedProperties = skippedProperties,
        };
    }

    public void Save(string fileName)
    {
        CommitChange();
        CreateXml(includeView: true).Save(fileName);

        FileName = fileName;
        Name = Path.GetFileNameWithoutExtension(fileName);
        _savedSnapshot = History.Current;
    }

    private static XmlDocument ParseXml(string text)
    {
        XmlDocument xml = new();
        xml.LoadXml(text);
        return xml;
    }

    /// <summary>Builds the sheet with its glyphs, connectors and groups from a document.</summary>
    private static Sheet ReadSheet(XmlDocument xml, string fileName, out bool skippedProperties)
    {
        XmlElement root = xml.DocumentElement ?? throw Invalid(fileName, "the document element is missing");
        XmlElement sheetNode = ChildElements(root, Settings.Xml.Tags.Sheet).FirstOrDefault()
            ?? throw Invalid(fileName, "the sheet element is missing");

        var sheet = new InfiniteSheet();
        if (TryReadView("ViewX", out float x) && TryReadView("ViewY", out float y) && TryReadView("Zoom", out float zoom) && zoom > 0)
        {
            sheet.ViewOrigin = new PointF(x, y);
            sheet.ScaleFactor = zoom;
        }

        bool TryReadView(string key, out float value) =>
            float.TryParse(sheetNode.GetAttribute(key), NumberStyles.Float, CultureInfo.InvariantCulture, out value) && float.IsFinite(value);

        skippedProperties = false;

        var glyphs = new List<AbstractGlyph>();
        foreach (XmlElement glyphNode in sheetNode.ChildNodes.OfType<XmlElement>())
        {
            AbstractGlyph glyph = CreateInstance<AbstractGlyph>(glyphNode.GetAttribute(Settings.Xml.Attributes.ClassName), fileName);
            skippedProperties |= glyph.Deserialize(glyphNode).Count > 0;
            glyphs.Add(glyph);
        }

        sheet.AddToBack(glyphs);

        foreach (ConnectorGlyph connector in glyphs.OfType<ConnectorGlyph>())
            connector.Relink(glyphs);

        sheet.RemoveOrphanConnectors();
        LoadGroups(root, sheet);

        return sheet;
    }

    private string CreateSnapshot() => CreateXml().OuterXml;

    private XmlDocument CreateXml(bool includeView = false, HashSet<AbstractGlyph>? selection = null)
    {
        var xml = new XmlDocument();

        XmlElement root = xml.CreateElement(Settings.Xml.Tags.FlowchartBuilderDocument, Settings.Xml.Namespace);
        root.SetAttribute(Settings.Xml.Attributes.ProgramVersion, typeof(Document).Assembly.GetName().Version?.ToString());
        xml.AppendChild(root);

        XmlElement sheetNode = xml.CreateElement(Settings.Xml.Tags.Sheet, Settings.Xml.Namespace);
        XmlAttribute className = xml.CreateAttribute(Settings.Xml.Attributes.ClassName, Settings.Xml.Namespace);
        className.Value = DocumentSheet.GetType().FullName;
        sheetNode.Attributes.Append(className);

        if (includeView)
        {
            sheetNode.SetAttribute("ViewX", XmlConvert.ToString(DocumentSheet.ViewOrigin.X));
            sheetNode.SetAttribute("ViewY", XmlConvert.ToString(DocumentSheet.ViewOrigin.Y));
            sheetNode.SetAttribute("Zoom", XmlConvert.ToString(DocumentSheet.ScaleFactor));
        }

        foreach (AbstractGlyph glyph in DocumentSheet.Glyphs)
            if (selection is null || selection.Contains(glyph))
                sheetNode.AppendChild(glyph.Serialize(xml));

        root.AppendChild(sheetNode);
        SaveGroups(xml, root, selection);
        return xml;
    }

    internal string? SelectionXml()
    {
        var selection = DocumentSheet.SelectedGlyphs.ToHashSet();
        foreach (var link in DocumentSheet.Glyphs.OfType<ConnectorGlyph>())
        {
            if (link.Source != null && link.Target != null && selection.Contains(link.Source) && selection.Contains(link.Target))
                selection.Add(link);
            else
                selection.Remove(link);
        }
        return selection.Count == 0 ? null : CreateXml(selection: selection).OuterXml;
    }

    internal static Sheet ReadClipboard(string text)
    {
        using var reader = XmlReader.Create(new StringReader(text), new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = Math.Max(1, text.Length),
        });
        var xml = new XmlDocument { XmlResolver = null };
        xml.Load(reader);
        if (xml.DocumentElement?.Name != Settings.Xml.Tags.FlowchartBuilderDocument ||
            xml.DocumentElement.NamespaceURI != Settings.Xml.Namespace)
            throw new InvalidDataException();
        var sheet = ReadSheet(xml, "Clipboard", out bool skipped);
        if (skipped) throw new InvalidDataException();
        foreach (var glyph in sheet.Glyphs) glyph.AssignNewId();
        foreach (var link in sheet.Glyphs.OfType<ConnectorGlyph>())
            link.Connect(link.Source!, link.SourcePort, link.Target!, link.TargetPort);
        return sheet;
    }

    /// <summary>Instantiates a type stored by its full name; it may live in this program or in a plugin.</summary>
    private static T CreateInstance<T>(string typeName, string fileName)
        where T : class
    {
        Type? type = string.IsNullOrEmpty(typeName)
            ? null
            : typeof(AbstractGlyph).Assembly.GetType(typeName) ?? Core.Instance.GlyphTypes.FirstOrDefault(t => t.FullName == typeName);

        if (type is null || type.IsAbstract || !type.IsAssignableTo(typeof(T)))
            throw Invalid(fileName, $"type '{typeName}' is not installed");

        return (T)Activator.CreateInstance(type)!;
    }

    private static void LoadGroups(XmlElement root, Sheet sheet)
    {
        Dictionary<string, BlockGlyph> blocks = sheet.Glyphs.OfType<BlockGlyph>().DistinctBy(glyph => glyph.Id).ToDictionary(glyph => glyph.Id);

        foreach (XmlElement groupNode in ChildElements(root, Settings.Xml.Tags.Groups).SelectMany(groups => ChildElements(groups, Settings.Xml.Tags.Group)))
        {
            List<BlockGlyph> group =
            [
                .. ChildElements(groupNode, Settings.Xml.Tags.GroupMember)
                    .Select(member => blocks.GetValueOrDefault(member.GetAttribute(Settings.Xml.Attributes.Id)))
                    .OfType<BlockGlyph>()
                    .Distinct(),
            ];

            if (group.Count >= 2)
                sheet.Groups.Add(group);
        }
    }

    private void SaveGroups(XmlDocument xml, XmlElement root, HashSet<AbstractGlyph>? selection)
    {
        if (DocumentSheet.Groups.Count == 0)
            return;

        XmlElement groupsNode = xml.CreateElement(Settings.Xml.Tags.Groups, Settings.Xml.Namespace);

        foreach (List<BlockGlyph> group in DocumentSheet.Groups)
        {
            XmlElement groupNode = xml.CreateElement(Settings.Xml.Tags.Group, Settings.Xml.Namespace);

            foreach (BlockGlyph member in group.Where(member => selection is null || selection.Contains(member)))
            {
                XmlElement memberNode = xml.CreateElement(Settings.Xml.Tags.GroupMember, Settings.Xml.Namespace);
                memberNode.SetAttribute(Settings.Xml.Attributes.Id, member.Id);
                groupNode.AppendChild(memberNode);
            }

            if (groupNode.ChildNodes.Count >= 2) groupsNode.AppendChild(groupNode);
        }

        root.AppendChild(groupsNode);
    }

    private static IEnumerable<XmlElement> ChildElements(XmlElement parent, string name) =>
        parent.ChildNodes.OfType<XmlElement>().Where(element => element.Name == name);

    private static InvalidDataException Invalid(string fileName, string reason, Exception? innerException = null) =>
        new($"'{fileName}' is not a valid FlowchartBuilder document: {reason.TrimEnd('.')}.", innerException);
}

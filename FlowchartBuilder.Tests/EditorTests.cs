using System.Xml;
using System.Xml.Linq;
using Makarov.FlowchartBuilder;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.Framework.Core.SettingStorages;
using Makarov.Framework.Graphics;
using Xunit;

namespace FlowchartBuilder.Tests;

public class EditorTests
{
    [Fact]
    public void NewEditAfterUndoDiscardsRedo()
    {
        var history = new DocumentHistory("a");
        Assert.True(history.Commit("b"));
        Assert.False(history.Commit("b"));
        Assert.Equal("a", history.Undo());
        Assert.True(history.Commit("c"));
        Assert.Null(history.Redo());
        Assert.Equal("a", history.Undo());
    }

    [Theory]
    [InlineData("ru-RU", "Russian")]
    [InlineData("uk-UA", "Ukrainian")]
    [InlineData("en-US", "English")]
    [InlineData("de-DE", "English")]
    public void FirstRunLanguageFollowsUiCulture(string culture, string expected)
    {
        var original = System.Globalization.CultureInfo.CurrentUICulture;
        try
        {
            System.Globalization.CultureInfo.CurrentUICulture = new(culture);
            Assert.Equal(expected, Settings.Environment.Languages.Default);
        }
        finally { System.Globalization.CultureInfo.CurrentUICulture = original; }
    }

    [Fact]
    public void EmbeddedImageRemainsAvailableAfterSavingReopeningAndUndo()
    {
        var core = Core.Initialize(new MemoryStorage());
        var type = core.GlyphTypes.Single(t => t.Name == "ImageGlyph");
        var image = (BlockGlyph)Activator.CreateInstance(type)!;
        using var bitmap = new Bitmap(3, 2);
        bitmap.SetPixel(1, 1, Color.Coral);
        using var stream = new MemoryStream();
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        string encoded = Convert.ToBase64String(stream.ToArray());
        var data = type.GetProperty("ImageData")!;
        data.SetValue(image, encoded);

        var document = new Document();
        document.DocumentSheet.Add(image);
        string file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mfb");
        try
        {
            document.Save(file);
            var loaded = Document.Load(file);
            var restored = Assert.Single(loaded.DocumentSheet.Glyphs);
            Assert.Equal(encoded, data.GetValue(restored));
            data.SetValue(restored, string.Empty);
            Assert.True(loaded.CommitChange());
            Assert.True(loaded.Undo());
            Assert.Equal(encoded, data.GetValue(Assert.Single(loaded.DocumentSheet.Glyphs)));
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public void XmlSettingsSurviveRestartAndRemoval()
    {
        string name = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
        try
        {
            new XmlStorage(name)["custom"] = "<данные>&";
            var reopened = new XmlStorage(name);
            Assert.Equal("<данные>&", reopened["custom"]);
            reopened["custom"] = null;
            Assert.Empty(new XmlStorage(name).Keys);
        }
        finally { File.Delete(name); }
    }

    [Theory]
    [InlineData(96, 96, 1)]
    [InlineData(97, 96, 2)]
    [InlineData(193, 97, 6)]
    public void PrintedTilesCoverContentOnce(int width, int height, int count)
    {
        var content = new Rectangle(-201, -19, width, height);
        var tiles = DocumentPrinter.Tiles(content, new Size(100, 100)).ToArray();
        Assert.Equal(count, tiles.Length);
        Assert.Equal(content, tiles.Aggregate(Rectangle.Union));
        Assert.Equal(width * height, tiles.Sum(t => t.Width * t.Height));
        for (int i = 0; i < tiles.Length; i++)
            Assert.DoesNotContain(tiles.Skip(i + 1), t => tiles[i].IntersectsWith(t));
    }

    [Theory]
    [InlineData(ConnectorRouting.Straight)]
    [InlineData(ConnectorRouting.Curved)]
    [InlineData(ConnectorRouting.Orthogonal)]
    public void RoutesJoinPortsAndSupportHitTesting(ConnectorRouting routing)
    {
        var start = new PointF(20, 10);
        var end = new PointF(120, 140);
        var route = ConnectorRoute.Create(routing, start, new(1, 0), new(0, 0, 20, 20),
            end, new(-1, 0), new(120, 130, 30, 20), 12);
        Assert.Equal(start, route.Start);
        Assert.Equal(end, route.End);
        Assert.InRange(route.DistanceTo(route.Midpoint), 0f, 0.01f);
        if (routing == ConnectorRouting.Orthogonal)
            Assert.All(route.Polyline.Zip(route.Polyline.Skip(1)), pair =>
                Assert.True(pair.First.X == pair.Second.X || pair.First.Y == pair.Second.Y));
    }

    [Fact]
    public void BitmapUploadPreservesBgraChannelOrder()
    {
        byte[] pixels = [0, 0, 255, 255, 255, 0, 0, 255];
        using var image = OffscreenRenderer.Render(2, 1, c => c.DrawBitmap(pixels, 2, 1, new(0, 0, 2, 1)));
        Assert.Equal(Color.Red.ToArgb(), image.GetPixel(0, 0).ToArgb());
        Assert.Equal(Color.Blue.ToArgb(), image.GetPixel(1, 0).ToArgb());
    }

    [Fact]
    public void ClipboardSuppliesXmlAsBothUnicodeTextAndCustomBytes()
    {
        const string xml = "<selection>Текст &amp; зображення</selection>";
        var data = GlyphClipboard.CreateData(xml);
        Assert.True(data.GetDataPresent(DataFormats.UnicodeText));
        Assert.Equal(xml, GlyphClipboard.Read(data));
        Assert.True(data.TryGetData<MemoryStream>(GlyphClipboard.Format, out var bytes));
        var onlyCustom = new DataObject();
        onlyCustom.SetData(GlyphClipboard.Format, false, bytes);
        Assert.Equal(xml, GlyphClipboard.Read(onlyCustom));
    }

    [Fact]
    public void CopyingTwoBlocksPreservesLinksGroupsAndTextButCreatesIndependentIds()
    {
        var core = Core.Initialize(new MemoryStorage());
        var type = core.GlyphTypes.Single(t => t.Name == "TextGlyph");
        var document = new Document();
        var sheet = document.DocumentSheet;
        var a = (BlockGlyph)Activator.CreateInstance(type)!;
        var b = (BlockGlyph)Activator.CreateInstance(type)!;
        a.Caption = "Ü <&> Блок";
        a.Selected = b.Selected = true;
        sheet.Add(a);
        sheet.Add(b);
        sheet.Add(new ConnectorGlyph(a, 1, b, 3));
        sheet.Groups.Add([a, b]);

        string xml = Assert.IsType<string>(document.SelectionXml());
        var first = Document.ReadClipboard(xml);
        var second = Document.ReadClipboard(xml);
        Assert.Equal(2, Assert.Single(first.Groups).Count);
        var link = Assert.Single(first.Glyphs.OfType<ConnectorGlyph>());
        Assert.Equal(a.Caption, link.Source!.Caption);
        Assert.Contains(link.Target!, first.Groups[0]);
        Assert.DoesNotContain(first.Glyphs, g => second.Glyphs.Any(copy => copy.Id == g.Id));
        b.Selected = false;
        Assert.Single(Document.ReadClipboard(document.SelectionXml()!).Glyphs);
    }

    [Fact]
    public void ClipboardRejectsExternalEntities()
    {
        Assert.Throws<XmlException>(() => Document.ReadClipboard("<!DOCTYPE x [<!ENTITY a SYSTEM 'file:///test'>]><x>&a;</x>"));
    }

    [Fact]
    public void DictionariesCoverMenusAndHaveIdenticalKeys()
    {
        var menus = Directory.GetFiles(Settings.Directories.Data, "*Menu.xml")
            .SelectMany(f => XDocument.Load(f).Descendants().Attributes("Caption")).Select(a => a.Value).Distinct().ToArray();
        var sets = Directory.GetFiles(Settings.Directories.Translations, "*.xml")
            .Select(f => XDocument.Load(f).Descendants().Attributes("Key").Select(a => a.Value).ToHashSet()).ToList();
        Assert.Equal(3, sets.Count);
        Assert.All(sets, keys => { Assert.True(keys.SetEquals(sets[0])); Assert.Empty(menus.Except(keys)); });
    }
}

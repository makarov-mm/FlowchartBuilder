using Tools = Makarov.FlowchartBuilder.Tools;
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
    public void ElectricalPaletteContainsAllSymbolsAndRendersEachThumbnail()
    {
        var core = Core.Initialize(new MemoryStorage());
        var glyphs = core.GlyphTypes.Where(t => GlyphMetadata.Of(t).Family == "Electrical")
            .Select(t => (AbstractGlyph)Activator.CreateInstance(t)!).ToArray();
        string[] expected = ["Resistor", "Capacitor", "Inductor", "Diode", "Ground", "DC voltage source",
            "AC voltage source", "Battery", "Lamp", "Switch", "Fuse", "NPN transistor", "Wire junction", "Wire"];
        Assert.Equal(expected, glyphs.OrderBy(g => g.Metadata.Order).Select(g => g.Name));
        foreach (var glyph in glyphs)
        {
            Assert.True(glyph.Metadata.IsVisible);
            Assert.True(core.CurrentTranslator.Contains(glyph.Name));
            using var icon = glyph.CreateThumbnail();
            Assert.Contains(Enumerable.Range(3, icon.Width - 6), x =>
                Enumerable.Range(3, icon.Height - 6).Any(y => icon.GetPixel(x, y).ToArgb() != icon.GetPixel(2, 2).ToArgb()));
        }
    }

    [Fact]
    public void CircuitWireKeepsRotatedEmitterAndGroundThroughDocumentRoundTrip()
    {
        var core = Core.Initialize(new MemoryStorage());
        BlockGlyph Create(string name) => (BlockGlyph)Activator.CreateInstance(
            core.GlyphTypes.Single(t => GlyphMetadata.Of(t).Name == name))!;
        var transistor = Create("NPN transistor");
        var ground = Create("Ground");
        transistor.XInMM = 40;
        transistor.YInMM = 15;
        transistor.Angle = 90;
        transistor.Caption = "Q1";
        var value = transistor.Metadata.FindActiveProperty("Value")!;
        value.SetValue(transistor, "BC547");
        var emitter = transistor.PortPosition(2, false);
        Assert.Equal(transistor.X - transistor.Height / 2, emitter.X, 3);
        Assert.Equal(transistor.Y + 0.55f * (transistor.Width / 2), emitter.Y, 3);
        Assert.Equal(-1f, transistor.PortDirection(2).X, 4);
        Assert.Equal(ground.PortPosition(0, false), ground.PortPosition(2, false));
        var wire = (ConnectorGlyph)Activator.CreateInstance(transistor.DefaultConnectorType)!;
        Assert.Equal(ConnectorRouting.Orthogonal, wire.Routing);
        Assert.False(wire.ArrowHead);
        wire.Connect(transistor, 2, ground, 0);
        var doc = new Document();
        doc.DocumentSheet.AddToBack([transistor, ground, wire]);
        Assert.True(doc.DocumentSheet.GetContentBounds().Contains(Point.Round(emitter)));
        string file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mfb");
        try
        {
            doc.Save(file);
            var restored = Assert.Single(Document.Load(file).DocumentSheet.Glyphs.OfType<ConnectorGlyph>());
            Assert.Equal("Wire", restored.Name);
            Assert.Equal(2, restored.SourcePort);
            Assert.Equal(emitter, restored.Source!.PortPosition(restored.SourcePort, false));
            Assert.Equal("BC547", value.GetValue(restored.Source));
            Assert.Equal(ground.Id, restored.Target!.Id);
        }
        finally { File.Delete(file); }
    }

    private static Document OpenExample(string file)
    {
        Core.Initialize(new MemoryStorage());
        return Document.Load(Path.Combine(AppContext.BaseDirectory, "Examples", file));
    }

    [Fact]
    public void RcExampleMatchesAnalyticGainAndPhase()
    {
        var network = Tools.CircuitSimulation.CircuitNetwork.From(OpenExample("RC-low-pass.mfb"));
        var model = Tools.CircuitSimulation.CircuitSimulator.Build(network);
        Assert.Empty(model.Warnings);
        var cap = network.Parts.Single(p => p.DisplayName == "C1");
        Assert.Equal(0, cap.NodeB);
        var atCutoff = model.SolveAc(1 / (2 * Math.PI * 1000 * 1e-6));
        var voltage = Tools.CircuitSimulation.CircuitSimulator.GetAcNodeVoltage(atCutoff, cap.NodeA);
        Assert.Equal(5 / Math.Sqrt(2), voltage.Magnitude, 4);
        Assert.Equal(-Math.PI / 4, voltage.Phase, 5);
        for (int i = 0; i < 50; i++) model.Step(0.0001);
        Assert.InRange(model.GetNodeVoltage(cap.NodeA), 4.4, 4.8);
        model.Reset();
        Assert.Equal(0, model.Time);
    }

    [Fact]
    public void DcDividerSolvesNodeVoltageAndDeliveredCurrent()
    {
        var source = new Tools.CircuitSimulation.CircuitPart("V1", Tools.CircuitSimulation.SymbolKind.DcSource, "9V", 1, 0);
        var r1 = new Tools.CircuitSimulation.CircuitPart("R1", Tools.CircuitSimulation.SymbolKind.Resistor, "1k", 1, 2);
        var r2 = new Tools.CircuitSimulation.CircuitPart("R2", Tools.CircuitSimulation.SymbolKind.Resistor, "2k", 2, 0);
        var model = Tools.CircuitSimulation.CircuitSimulator.Build(new(["GND", "supply", "output"], [source, r1, r2]));
        model.SolveOperatingPoint();
        Assert.Equal(6, model.GetNodeVoltage(2), 4);
        Assert.Equal(0.003, model.GetCurrent(source)!.Value, 7);
        Assert.Equal(0.003, model.GetCurrent(r2)!.Value, 7);
    }

    [Fact]
    public void BridgeChargesReservoirOnBothHalfCycles()
    {
        var parts = new List<Tools.CircuitSimulation.CircuitPart>
        {
            new("V1", Tools.CircuitSimulation.SymbolKind.AcSource, "15V 50Hz", 1, 2),
            new("D1", Tools.CircuitSimulation.SymbolKind.Diode, "", 1, 3),
            new("D2", Tools.CircuitSimulation.SymbolKind.Diode, "", 2, 3),
            new("D3", Tools.CircuitSimulation.SymbolKind.Diode, "", 0, 1),
            new("D4", Tools.CircuitSimulation.SymbolKind.Diode, "", 0, 2),
            new("C1", Tools.CircuitSimulation.SymbolKind.Capacitor, "220uF", 3, 0),
            new("R1", Tools.CircuitSimulation.SymbolKind.Resistor, "1k", 3, 0)
        };
        var model = Tools.CircuitSimulation.CircuitSimulator.Build(new(["GND", "AC1", "AC2", "out"], parts));
        for (int i = 0; i < 150; i++)
        {
            model.Step(0.0001);
            if (i is 49 or 149) Assert.InRange(model.GetNodeVoltage(3), 13.4, 13.7);
        }
    }

    [Fact]
    public void SwitchCurrentTracksSessionToggleAndOverloadBlowsFuse()
    {
        var source = new Tools.CircuitSimulation.CircuitPart("V", Tools.CircuitSimulation.SymbolKind.DcSource, "5V", 1, 0);
        var sw = new Tools.CircuitSimulation.CircuitPart("S", Tools.CircuitSimulation.SymbolKind.Switch, "", 1, 2);
        var load = new Tools.CircuitSimulation.CircuitPart("R", Tools.CircuitSimulation.SymbolKind.Resistor, "10", 2, 3);
        var fuse = new Tools.CircuitSimulation.CircuitPart("F", Tools.CircuitSimulation.SymbolKind.Fuse, "0.25A", 3, 0);
        var model = Tools.CircuitSimulation.CircuitSimulator.Build(new(["GND", "supply", "switch", "load"], [source, sw, load, fuse]));
        model.Step(1e-4);
        Assert.False(model.IsFuseBlown(fuse));
        sw.StateOn = true;
        model.Step(1e-4);
        Assert.InRange(model.GetCurrent(sw)!.Value, 0.49, 0.51);
        Assert.True(model.IsFuseBlown(fuse));
        model.Reset();
        Assert.False(model.IsFuseBlown(fuse));
    }

    [Fact]
    public void SimulationRejectsMissingGround()
    {
        var doc = OpenExample("RC-low-pass.mfb");
        var ground = doc.DocumentSheet.Glyphs.Single(g => g.Name == "Ground");
        ground.Selected = true;
        doc.DocumentSheet.DeleteSelectedGlyphs();
        Assert.Contains("Ground", Assert.Throws<InvalidDataException>(() => Tools.CircuitSimulation.CircuitNetwork.From(doc)).Message);
    }

    [Fact]
    public void CountdownExportPreservesBothBranchesAndLoopBack()
    {
        var code = Tools.PythonExport.PythonGenerator.Generate(OpenExample("Countdown.mfb"));
        Assert.Contains("n = 3", code);
        Assert.Contains("_flow_step = 3 if (n > 0) else 4", code);
        Assert.Contains("print(n)", code);
        Assert.Contains("n -= 1" + Environment.NewLine + "            _flow_step = 2", code);
    }

    [Fact]
    public void ExportRejectsAmbiguousDecisions()
    {
        var doc = OpenExample("Countdown.mfb");
        doc.DocumentSheet.Glyphs.OfType<ConnectorGlyph>().Single(e => e.Caption == "false").Caption = "true";
        Assert.Throws<InvalidDataException>(() => Tools.PythonExport.PythonGenerator.Generate(doc));
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

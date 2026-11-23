using System.ComponentModel;
using System.Drawing.Design;
using Makarov.FlowchartBuilder;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.Framework.Core.SettingStorages;
using Xunit;

namespace FlowchartBuilder.Tests;

[Collection("Editor")]
public class UmlGlyphTests
{
    private static AbstractGlyph Create(string name)
    {
        var core = Core.Initialize(new MemoryStorage());
        var type = core.GlyphTypes.Single(type => GlyphMetadata.Of(type).Name == name);
        return (AbstractGlyph)Activator.CreateInstance(type)!;
    }

    [Fact]
    public void PaletteShowsDistinctRelationshipSymbolsAndSupportsMultilineEditing()
    {
        var type = (BlockGlyph)Create("UML class");
        var association = (ConnectorGlyph)Create("UML association");
        var inheritance = (ConnectorGlyph)Create("UML inheritance");
        Assert.All(new AbstractGlyph[] { type, association, inheritance }, glyph =>
        {
            Assert.Equal("UML", glyph.Family);
            Assert.True(glyph.Metadata.IsVisible);
        });
        Assert.Equal(association.GetType(), type.DefaultConnectorType);
        Assert.True(type.SupportsLinks);
        foreach (var connector in new[] { association, inheritance })
        {
            Assert.True(connector.AcceptsEndpoint(type));
            Assert.False(connector.AcceptsEndpoint((BlockGlyph)Create("Begin")));
            Assert.Null(TypeDescriptor.GetProperties(connector)[nameof(ConnectorGlyph.ArrowHead)]);
        }
        foreach (string section in new[] { "Attributes", "Methods" })
            Assert.IsAssignableFrom<UITypeEditor>(TypeDescriptor.GetProperties(type)[section]!.GetEditor(typeof(UITypeEditor)));

        using var classIcon = type.CreateThumbnail();
        using var assocIcon = association.CreateThumbnail();
        using var inheritIcon = inheritance.CreateThumbnail();
        Assert.NotEqual(classIcon.GetPixel(5, 8), classIcon.GetPixel(2, 2));
        Assert.Contains(Enumerable.Range(4, assocIcon.Width - 8), x =>
            Enumerable.Range(4, assocIcon.Height - 8).Any(y => assocIcon.GetPixel(x, y) != inheritIcon.GetPixel(x, y)));
    }

    [Fact]
    public void DocumentAndClipboardKeepMembersRolesAndBothRelationshipDirections()
    {
        var child = (BlockGlyph)Create("UML class");
        var parent = (BlockGlyph)Create("UML class");
        child.Caption = "Task";
        parent.Caption = "Item";
        var attributes = child.Metadata.FindActiveProperty("Attributes")!;
        var methods = child.Metadata.FindActiveProperty("Methods")!;
        attributes.SetValue(child, "title: str\r\nsize: int");
        methods.SetValue(child, "start()\nstop()");
        var inheritance = (ConnectorGlyph)Create("UML inheritance");
        inheritance.Connect(child, 0, parent, 2);
        var association = (ConnectorGlyph)Create("UML association");
        association.Connect(parent, 1, child, 3);
        association.Caption = "task";

        var document = new Document();
        document.DocumentSheet.AddToBack([child, parent, inheritance, association]);
        child.Selected = parent.Selected = true;
        var copied = Document.ReadClipboard(document.SelectionXml()!);
        Assert.DoesNotContain(copied.Glyphs, glyph => document.DocumentSheet.Glyphs.Any(original => original.Id == glyph.Id));
        Check(copied.Glyphs);

        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mfb");
        try
        {
            document.Save(path);
            var loaded = Document.Load(path);
            Assert.Equal(document.DocumentSheet.Glyphs.Select(glyph => glyph.Id), loaded.DocumentSheet.Glyphs.Select(glyph => glyph.Id));
            Check(loaded.DocumentSheet.Glyphs);
        }
        finally { File.Delete(path); }

        void Check(IReadOnlyList<AbstractGlyph> glyphs)
        {
            var loadedChild = glyphs.OfType<BlockGlyph>().Single(glyph => glyph.Caption == "Task");
            var loadedParent = glyphs.OfType<BlockGlyph>().Single(glyph => glyph.Caption == "Item");
            Assert.Equal("title: str\nsize: int", attributes.GetValue(loadedChild));
            Assert.Equal("start()\nstop()", methods.GetValue(loadedChild));
            var inherits = glyphs.OfType<ConnectorGlyph>().Single(edge => edge.Name == "UML inheritance");
            Assert.Same(loadedChild, inherits.Source);
            Assert.Same(loadedParent, inherits.Target);
            Assert.Equal((0, 2), (inherits.SourcePort, inherits.TargetPort));
            var owns = glyphs.OfType<ConnectorGlyph>().Single(edge => edge.Name == "UML association");
            Assert.Same(loadedParent, owns.Source);
            Assert.Same(loadedChild, owns.Target);
            Assert.Equal((1, 3), (owns.SourcePort, owns.TargetPort));
            Assert.Equal("task", owns.Caption);
        }
    }
}

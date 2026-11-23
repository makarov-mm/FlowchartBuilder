using System.ComponentModel;
using System.Drawing.Design;
using Makarov.FlowchartBuilder;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Glyphs.EntityRelationship;
using Makarov.Framework.Core.SettingStorages;
using Xunit;

namespace FlowchartBuilder.Tests;

[Collection("Editor")]
public sealed class ErGlyphTests
{
    [Fact]
    public void DatabasePaletteSupportsMultilineFieldsAndTableOnlyRelationships()
    {
        var core = Core.Initialize(new MemoryStorage());
        Assert.Contains(typeof(TableGlyph), core.GlyphTypes);
        Assert.Contains(typeof(RelationshipGlyph), core.GlyphTypes);
        var table = new TableGlyph();
        var link = new RelationshipGlyph();
        Assert.Equal(link.GetType(), table.DefaultConnectorType);
        Assert.True(table.SupportsLinks);
        Assert.True(link.AcceptsEndpoint(table));
        Assert.False(link.AcceptsEndpoint(new OtherBlock()));
        Assert.True(link.AllowsSelfConnection);
        Assert.Null(TypeDescriptor.GetProperties(link)[nameof(ConnectorGlyph.ArrowHead)]);
        Assert.IsAssignableFrom<UITypeEditor>(TypeDescriptor.GetProperties(table)[nameof(table.Fields)]!.GetEditor(typeof(UITypeEditor)));
        Assert.Single(table.Metadata.ActiveProperties, p => p.Name == nameof(table.Caption));
        Assert.Single(link.Metadata.ActiveProperties, p => p.Name == nameof(link.Caption));
        using var tableIcon = table.CreateThumbnail();
        using var linkIcon = link.CreateThumbnail();
        Assert.Contains(Enumerable.Range(4, linkIcon.Width - 8), x =>
            Enumerable.Range(4, linkIcon.Height - 8).Any(y => tableIcon.GetPixel(x, y) != linkIcon.GetPixel(x, y)));
    }

    [Fact]
    public void LibraryForeignKeysAndConditionalUniqueRuleSurviveCopyAndSave()
    {
        Core.Initialize(new MemoryStorage());
        var document = Document.LoadExample(Path.Combine(AppContext.BaseDirectory, "Examples", "Library-database.mfb"));
        Assert.False(document.HasSkippedProperties);
        Check(document.DocumentSheet.Glyphs);
        foreach (var table in document.DocumentSheet.Glyphs.OfType<TableGlyph>()) table.Selected = true;
        var clipboard = Document.ReadClipboard(document.SelectionXml()!);
        Check(clipboard.Glyphs);
        Assert.DoesNotContain(clipboard.Glyphs, copy => document.DocumentSheet.Glyphs.Any(original => original.Id == copy.Id));

        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mfb");
        try
        {
            document.Save(path);
            var loaded = Document.Load(path);
            Assert.False(loaded.HasSkippedProperties);
            Check(loaded.DocumentSheet.Glyphs);
        }
        finally { File.Delete(path); }

        static void Check(IReadOnlyList<AbstractGlyph> glyphs)
        {
            var tables = glyphs.OfType<TableGlyph>().ToDictionary(table => table.Caption);
            Assert.Equal(4, tables.Count);
            var loans = tables["loans"];
            Assert.Equal(6, loans.Fields.Split('\n').Length);
            Assert.Equal("copy_id", loans.UniqueField);
            Assert.Equal("returned_on", loans.WhileNullField);
            var links = glyphs.OfType<RelationshipGlyph>().ToDictionary(link => link.ChildField);
            Assert.Equal(3, links.Count);
            Assert.Same(tables["books"], links["book_id"].Source);
            Assert.Same(tables["copies"], links["book_id"].Target);
            Assert.Same(tables["copies"], links["copy_id"].Source);
            Assert.Same(loans, links["copy_id"].Target);
            Assert.Same(tables["readers"], links["reader_id"].Source);
            Assert.Same(loans, links["reader_id"].Target);
        }
    }

    private sealed class OtherBlock : BlockGlyph;
}

using Makarov.FlowchartBuilder;
using Makarov.FlowchartBuilder.Glyphs.EntityRelationship;
using Makarov.FlowchartBuilder.Tools.EntityRelationship;
using Makarov.Framework.Core.SettingStorages;
using Xunit;

namespace FlowchartBuilder.Tests;

[Collection("Editor")]
public sealed class ErDocumentTests
{
    private static Document OpenLibrary()
    {
        Core.Initialize(new MemoryStorage());
        return Document.LoadExample(Path.Combine(AppContext.BaseDirectory, "Examples", "Library-database.mfb"));
    }

    [Fact]
    public void LibraryExampleEnforcesLoansAndKeepsTheirHistory()
    {
        var document = OpenLibrary();
        var schema = EntityDiagramReader.Analyze(document, out bool library);
        Assert.True(library);
        Assert.Empty(schema.Errors);
        using var db = new SandboxDatabase(schema.Sql!);
        db.Execute(SandboxExamples.Seed);
        Assert.Single(db.Execute(SandboxExamples.Issue(1, 1)).Rows);
        Assert.Throws<InvalidOperationException>(() => db.Execute(SandboxExamples.Issue(1, 2)));
        Assert.Equal(2, db.Execute(SandboxExamples.Issue(2, 2)).Rows.Count);
        db.Execute(SandboxExamples.ReturnFirst);
        Assert.Equal(3, db.Execute(SandboxExamples.Issue(1, 2)).Rows.Count);
        Assert.Throws<InvalidOperationException>(() => db.Execute("UPDATE loans SET reader_id = 99 WHERE id = 1"));
        Assert.Equal("3", db.Execute("SELECT count(*) FROM loans").Rows.Single()[0]);
        Assert.False(document.IsModified);
    }

    [Fact]
    public void BrokenFieldAndConditionalRuleAreLinkedToTheDiagramAndUndoRestoresSql()
    {
        var document = OpenLibrary();
        var original = EntityDiagramReader.Analyze(document, out _);
        Assert.Empty(original.Errors);
        var relation = document.DocumentSheet.Glyphs.OfType<RelationshipGlyph>().Single(r => r.ChildField == "copy_id");
        var loans = document.DocumentSheet.Glyphs.OfType<TableGlyph>().Single(t => t.Caption == "loans");
        loans.UniqueField = "gone";
        relation.ChildField = "missing_key";
        Assert.True(document.CommitChange());
        var invalid = EntityDiagramReader.Analyze(document, out _);
        Assert.Null(invalid.Sql);
        Assert.Contains(invalid.Errors, e => e.Id == loans.Id);
        Assert.Contains(invalid.Errors, e => e.Id == relation.Id);
        Assert.True(document.Undo());
        Assert.Equal(original.Sql, EntityDiagramReader.Analyze(document, out _).Sql);
        Assert.True(document.Redo());
        Assert.False(EntityDiagramReader.Analyze(document, out _).IsValid);
    }
}

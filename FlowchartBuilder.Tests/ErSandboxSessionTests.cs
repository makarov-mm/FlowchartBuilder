using Makarov.FlowchartBuilder.Tools.EntityRelationship;
using Xunit;

namespace FlowchartBuilder.Tests;

public sealed class ErSandboxSessionTests
{
    private const string Schema = "CREATE TABLE copies(id INTEGER PRIMARY KEY);";
    private const string Extended = Schema + "CREATE TABLE loans(id INTEGER PRIMARY KEY);";

    [Fact]
    public void UndoRestoresTheSameRowsAndAnExplicitRebuildActivatesTheLatestSchema()
    {
        using var session = new SandboxSession();
        session.SetSchema(Schema, false);
        session.Rebuild();
        session.Execute("INSERT INTO copies VALUES(42)");
        session.SetSchema(Extended, false);
        Assert.False(session.CanRun);
        Assert.Throws<InvalidOperationException>(() => session.Execute("DELETE FROM copies"));
        session.SetSchema(null, false);
        Assert.False(session.CanRun);
        session.SetSchema(Schema, false);
        Assert.Equal("42", Assert.Single(session.Execute("SELECT id FROM copies").Rows)[0]);

        session.SetSchema(Extended, false);
        session.Rebuild();
        Assert.True(session.CanRun);
        Assert.Empty(session.Execute("SELECT * FROM copies").Rows);
        Assert.Empty(session.Execute("SELECT * FROM loans").Rows);
    }

    [Fact]
    public void FailedRebuildLeavesTheOriginalDatabaseAvailableAfterUndo()
    {
        using var session = new SandboxSession();
        session.SetSchema(Schema, false);
        session.Rebuild();
        session.Execute("INSERT INTO copies VALUES(7)");
        session.SetSchema("CREATE TABLE malformed(", false);
        Assert.Throws<InvalidOperationException>(() => session.Rebuild());
        Assert.False(session.CanRun);
        session.SetSchema(Schema, false);
        Assert.Equal("7", Assert.Single(session.Execute("SELECT id FROM copies").Rows)[0]);
    }

    [Fact]
    public void IncompatibleLibrarySeedRollsBackAndLeavesTheEditedSchemaUsable()
    {
        using var session = new SandboxSession();
        session.SetSchema("CREATE TABLE books(id INTEGER PRIMARY KEY, title TEXT);", true);
        Assert.NotNull(session.Rebuild());
        Assert.False(session.HasLibraryData);
        Assert.True(session.CanRun);
        Assert.Empty(session.Execute("SELECT * FROM books").Rows);
    }
}

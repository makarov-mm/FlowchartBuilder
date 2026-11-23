using Makarov.FlowchartBuilder.Tools.EntityRelationship;
using Xunit;

namespace FlowchartBuilder.Tests;

public class ErSandboxTests
{
    private const string Tables = """
        CREATE TABLE books (id INTEGER PRIMARY KEY, title TEXT NOT NULL);
        CREATE TABLE readers (id INTEGER PRIMARY KEY, name TEXT NOT NULL);
        CREATE TABLE copies (id INTEGER PRIMARY KEY, book_id INTEGER NOT NULL REFERENCES books(id));
        CREATE TABLE loans (id INTEGER PRIMARY KEY, copy_id INTEGER NOT NULL REFERENCES copies(id),
            reader_id INTEGER NOT NULL REFERENCES readers(id), issued_on TEXT NOT NULL, due_on TEXT NOT NULL, returned_on TEXT);
        CREATE UNIQUE INDEX one_open ON loans(copy_id) WHERE returned_on IS NULL;
        """;

    [Fact]
    public void LibraryScenarioUsesTheSameQueriesAsTheEditor()
    {
        using var db = new SandboxDatabase(Tables);
        Assert.Equal(5, db.Execute(SandboxExamples.Seed).Changes);
        var first = db.Execute(SandboxExamples.Issue(1, 1));
        Assert.Equal("Анна", Assert.Single(first.Rows)[3]);
        Assert.Null(first.Rows[0][6]);
        Assert.Contains("UNIQUE", Assert.Throws<InvalidOperationException>(() => db.Execute(SandboxExamples.Issue(1, 2))).Message);
        Assert.Equal(2, db.Execute(SandboxExamples.Issue(2, 2)).Rows.Count);
        Assert.NotNull(db.Execute(SandboxExamples.ReturnFirst).Rows[0][6]);
        var again = db.Execute(SandboxExamples.Issue(1, 2));
        Assert.Equal(3, again.Rows.Count);
        Assert.Equal("Олег", again.Rows[2][3]);
        Assert.Null(again.Rows[2][6]);

        using var reset = new SandboxDatabase(Tables);
        Assert.Empty(reset.Execute(SandboxExamples.Loans).Rows);
    }

    [Fact]
    public void FailedBatchRevertsEarlierStatementsAndLeavesConnectionUsable()
    {
        using var db = new SandboxDatabase(Tables);
        db.Execute(SandboxExamples.Seed);
        var error = Assert.Throws<InvalidOperationException>(() => db.Execute("""
            INSERT INTO books(title) VALUES ('Test');
            INSERT INTO copies(book_id) VALUES (999);
            """));
        Assert.Contains("FOREIGN KEY", error.Message);
        Assert.Equal("1", Assert.Single(db.Execute("SELECT count(*) FROM books;").Rows)[0]);
        Assert.Equal(1, db.Execute("UPDATE books SET title = 'Робинзон';").Changes);
    }

    [Theory]
    [InlineData("ATTACH DATABASE ':memory:' AS another;")]
    [InlineData("PRAGMA foreign_keys = OFF;")]
    [InlineData("VACUUM INTO ':memory:';")]
    [InlineData("COMMIT;")]
    [InlineData("DROP TABLE books;")]
    public void UserCommandsKeepChangesInsideTheTemporarySchema(string sql)
    {
        using var db = new SandboxDatabase(Tables);
        Assert.Throws<InvalidOperationException>(() => db.Execute(sql));
        Assert.Equal("1", Assert.Single(db.Execute("PRAGMA foreign_keys;").Rows)[0]);
    }

    [Fact]
    public void TablePreviewsUseExactDiagramNamesAndReadOnlyTheFirstPage()
    {
        var schema = SchemaCompiler.Compile([
            new("a", "Книга", [new("text", "TEXT")]),
            new("b", "книга", [new("text", "TEXT")]),
            new("c", "A \"quoted\" title", [new("text", "TEXT")])
        ], []);
        Assert.Empty(schema.Errors);
        using var db = new SandboxDatabase(schema.Sql!);
        Assert.Equal(new[] { "A \"quoted\" title", "Книга", "книга" }, db.TableNames);
        db.Execute("""
            INSERT INTO "Книга" VALUES ('first');
            INSERT INTO "книга" VALUES ('second');
            WITH RECURSIVE n(i) AS (VALUES(1) UNION ALL SELECT i+1 FROM n WHERE i<250)
            INSERT INTO "A ""quoted"" title" SELECT i FROM n;
            """);
        Assert.Equal("first", Assert.Single(db.Execute(SandboxExamples.ReadTable("Книга")).Rows)[0]);
        Assert.Equal("second", Assert.Single(db.Execute(SandboxExamples.ReadTable("книга")).Rows)[0]);
        var preview = db.Execute(SandboxExamples.ReadTable("A \"quoted\" title"));
        Assert.Equal(SandboxDatabase.DisplayRows, preview.Rows.Count);
        Assert.True(preview.MoreRows);
        Assert.Equal("250", Assert.Single(db.Execute("""SELECT count(*) FROM "a ""quoted"" title";""").Rows)[0]);
    }

    [Fact]
    public void EmptyResultsHaveHeadersAndLargeResultsHaveAPreview()
    {
        using var db = new SandboxDatabase(Tables);
        var empty = db.Execute("SELECT id, title FROM books;");
        Assert.Equal(new[] { "id", "title" }, empty.Columns);
        Assert.Empty(empty.Rows);
        var sameNames = db.Execute("SELECT 'book' AS name, 'reader' AS name;");
        Assert.Equal(new[] { "name", "name" }, sameNames.Columns);
        Assert.Equal(new[] { "book", "reader" }, Assert.Single(sameNames.Rows));
        var many = db.Execute("""
            WITH RECURSIVE numbers(n) AS (VALUES(1) UNION ALL SELECT n+1 FROM numbers WHERE n < 500)
            SELECT n FROM numbers;
            """);
        Assert.Equal(SandboxDatabase.DisplayRows, many.Rows.Count);
        Assert.True(many.MoreRows);
        Assert.Equal("1", many.Rows[0][0]);
    }
}

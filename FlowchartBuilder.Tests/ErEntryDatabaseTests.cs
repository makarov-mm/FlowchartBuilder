using Makarov.FlowchartBuilder.Tools.EntityRelationship;
using Xunit;

namespace FlowchartBuilder.Tests;

public sealed class ErEntryDatabaseTests
{
    [Fact]
    public void LibraryFieldsIncludeOptionalDatesAutomaticIdsAndNamedReferences()
    {
        using var db = new SandboxDatabase("""
            CREATE TABLE books (id INTEGER PRIMARY KEY, title TEXT);
            CREATE TABLE readers (id INTEGER PRIMARY KEY, name TEXT);
            CREATE TABLE copies (id INTEGER PRIMARY KEY, book_id INTEGER REFERENCES books(id));
            CREATE TABLE loans (id INTEGER PRIMARY KEY, copy_id INTEGER NOT NULL REFERENCES copies(id),
                                reader_id INTEGER NOT NULL REFERENCES readers(id), returned_on TEXT);
            CREATE UNIQUE INDEX open_copy ON loans(copy_id) WHERE returned_on IS NULL;
            """);
        db.Execute(SandboxExamples.Seed);
        var columns = db.GetEntryColumns("loans");
        Assert.Equal(new[] { "id", "copy_id", "reader_id", "returned_on" }, columns.Select(c => c.Name));
        Assert.True(columns[0].AutoId);
        Assert.True(columns[1].Required);
        Assert.False(columns[3].Required);
        Assert.Equal(new EntryReference("1", "Робинзон Крузо"),
            Assert.Single(db.GetReferenceValues(db.GetEntryColumns("copies")[1])));
        Assert.Equal(new EntryReference("1", "Анна"), db.GetReferenceValues(columns[2])[0]);
        Assert.Equal(new[] { new EntryReference("1", null), new EntryReference("2", null) }, db.GetReferenceValues(columns[1]));

        var loan = new Dictionary<string, EntryValue> { ["copy_id"] = new("1"), ["reader_id"] = new("2") };
        Assert.Equal(1L, db.InsertRow("loans", loan));
        Assert.Contains("UNIQUE", Assert.Throws<InvalidOperationException>(() => db.InsertRow("loans", loan)).Message);
        loan["copy_id"] = new("2");
        loan["reader_id"] = new("999");
        Assert.Contains("FOREIGN KEY", Assert.Throws<InvalidOperationException>(() => db.InsertRow("loans", loan)).Message);
        loan["reader_id"] = new("1");
        Assert.Equal(2L, db.InsertRow("loans", loan));
        Assert.Equal("2", Assert.Single(db.Execute("SELECT count(*) FROM loans;").Rows)[0]);
    }

    [Fact]
    public void InsertsRespectDefaultsExplicitNullsAndEmptyValues()
    {
        using var db = new SandboxDatabase(""""
            CREATE TABLE "My ""records""" (id INTEGER PRIMARY KEY, "copy's name" TEXT DEFAULT 'unnamed', bytes BLOB);
            """");
        const string name = "My \"records\"";
        db.InsertRow(name, new Dictionary<string, EntryValue>());
        db.InsertRow(name, new Dictionary<string, EntryValue> { ["copy's name"] = new(null), ["bytes"] = new("") });
        db.InsertRow(name, new Dictionary<string, EntryValue> { ["copy's name"] = new(""), ["bytes"] = new("0100fe") });
        const string text = "\uFEFF Пётр O’Neill's \"книга\"\n🪶\0после ";
        db.InsertRow(name, new Dictionary<string, EntryValue> { ["copy's name"] = new(text) });
        var rows = db.Execute(""""SELECT "copy's name", typeof(bytes), length(bytes), hex(bytes) FROM "My ""records""" ORDER BY id;"""").Rows;
        Assert.Equal(new string?[] { "unnamed", "null", null, "" }, rows[0]);
        Assert.Equal(new string?[] { null, "blob", "0", "" }, rows[1]);
        Assert.Equal(new string?[] { "", "blob", "3", "0100FE" }, rows[2]);
        Assert.Equal(text, rows[3][0]);
    }

    [Fact]
    public void NumericBindingsPreserveFullIntegerPrecision()
    {
        using var db = new SandboxDatabase("CREATE TABLE n (big INTEGER, exact NUMERIC, floating REAL);");
        db.InsertRow("n", new Dictionary<string, EntryValue>
        {
            ["big"] = new("-9223372036854775808"), ["exact"] = new("9223372036854775807"), ["floating"] = new("1.25e2")
        });
        Assert.Equal(new[] { "-9223372036854775808", "9223372036854775807", "integer", "125.0" },
            Assert.Single(db.Execute("SELECT big, exact, typeof(exact), floating FROM n;").Rows));
    }

    [Theory]
    [InlineData("i", "9223372036854775808")]
    [InlineData("i", "1.2")]
    [InlineData("r", "1,5")]
    [InlineData("n", "Infinity")]
    [InlineData("b", "a")]
    [InlineData("b", "xy")]
    [InlineData("t", null)]
    public void InvalidFormValuesLeaveTheSessionUsable(string column, string? value)
    {
        using var db = new SandboxDatabase("CREATE TABLE t (i INTEGER, r REAL, n NUMERIC, b BLOB, t TEXT NOT NULL);");
        var fields = new Dictionary<string, EntryValue> { ["t"] = new("valid") };
        fields[column] = new(value);
        Assert.Equal(column, Assert.Throws<EntryInputException>(() => db.InsertRow("t", fields)).Column.Name);
        Assert.Throws<ArgumentException>(() => db.InsertRow("t", new Dictionary<string, EntryValue> { ["unknown"] = new("v") }));
        Assert.Equal(1L, db.InsertRow("t", new Dictionary<string, EntryValue> { ["t"] = new("valid") }));
    }

    [Fact]
    public void ReferenceSuggestionsRoundTripPreciseRealBinaryAndLongTextKeys()
    {
        using var db = new SandboxDatabase("""
            CREATE TABLE r (k REAL PRIMARY KEY);
            CREATE TABLE b (k BLOB PRIMARY KEY);
            CREATE TABLE t (k TEXT PRIMARY KEY, title TEXT);
            CREATE TABLE c (r REAL REFERENCES r(k), b BLOB REFERENCES b(k), t TEXT REFERENCES t(k));
            """);
        string longKey = new('я', 1500);
        db.InsertRow("r", new Dictionary<string, EntryValue> { ["k"] = new("1.0000000000000002") });
        db.InsertRow("b", new Dictionary<string, EntryValue> { ["k"] = new("00fe") });
        db.InsertRow("t", new Dictionary<string, EntryValue> { ["k"] = new(longKey), ["title"] = new(new string('L', 100)) });
        var input = db.GetEntryColumns("c").ToDictionary(c => c.Name, c => Assert.Single(db.GetReferenceValues(c)));
        Assert.Equal("1.0000000000000002", input["r"].Key);
        Assert.Equal("00FE", input["b"].Key);
        Assert.Equal(longKey, input["t"].Key);
        Assert.Equal(64, input["t"].Label!.Length);
        Assert.Equal(1L, db.InsertRow("c", input.ToDictionary(kv => kv.Key, kv => new EntryValue(kv.Value.Key))));
    }

    [Theory]
    [InlineData("id INTEGER PRIMARY KEY, rowid TEXT, _rowid_ TEXT, oid TEXT", "id")]
    [InlineData("rowid TEXT, item TEXT", "_rowid_")]
    public void LargeTablePreviewStartsWithNewRowAndIncludesEarlierRows(string fields, string key)
    {
        using var db = new SandboxDatabase($"CREATE TABLE page ({fields});");
        db.Execute($"WITH RECURSIVE n(i) AS (VALUES(1) UNION ALL SELECT i+1 FROM n WHERE i < 220) INSERT INTO page({key}) SELECT i FROM n;");
        var rowId = db.InsertRow("page", new Dictionary<string, EntryValue> { ["rowid"] = new("latest") });
        var preview = db.PreviewInsertedRow("page", rowId);
        Assert.Equal(221L, rowId);
        Assert.Equal(SandboxDatabase.DisplayRows, preview.Rows.Count);
        Assert.True(preview.MoreRows);
        Assert.Contains("latest", preview.Rows[0]);
        Assert.DoesNotContain(preview.Rows.Skip(1), row => row.Contains("latest"));
    }

    [Fact]
    public void SuggestionsAreBoundedButOtherForeignKeysCanBeEntered()
    {
        using var db = new SandboxDatabase("CREATE TABLE p (id INTEGER PRIMARY KEY); CREATE TABLE c (id INTEGER REFERENCES p(id));");
        db.Execute("WITH RECURSIVE n(i) AS (VALUES(1) UNION ALL SELECT i+1 FROM n WHERE i<120) INSERT INTO p SELECT i FROM n;");
        var keys = db.GetReferenceValues(Assert.Single(db.GetEntryColumns("c")));
        Assert.Equal(100, keys.Count);
        Assert.Equal("100", keys[^1].Key);
        Assert.Equal(1L, db.InsertRow("c", new Dictionary<string, EntryValue> { ["id"] = new("120") }));
    }
}

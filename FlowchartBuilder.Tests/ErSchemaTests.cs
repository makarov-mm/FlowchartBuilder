using Makarov.FlowchartBuilder.Tools.EntityRelationship;
using Xunit;

namespace FlowchartBuilder.Tests;

public class ErSchemaTests
{
    [Fact]
    public void FieldTextSupportsQuotedNamesAndReportsLineNumbers()
    {
        var diagnostics = new List<SchemaDiagnostic>();
        var fields = SchemaFields.Parse("t", "id integer primary key\r\n\r\n\"a \"\"label\"\"\" text unique NOT NULL\nfree Blob", diagnostics);
        Assert.Empty(diagnostics);
        Assert.Equal(new SchemaField("id", "INTEGER", PrimaryKey: true, Nullable: false), fields[0]);
        Assert.Equal(new SchemaField("a \"label\"", "TEXT", Nullable: false, Unique: true), fields[1]);
        Assert.Equal(new SchemaField("free", "BLOB"), fields[2]);

        SchemaFields.Parse("t", "\nmissing\nbad INTEGER DEFAULT 0\ndouble TEXT UNIQUE UNIQUE", diagnostics);
        Assert.Contains(diagnostics, d => d.Message.Contains("line 2"));
        Assert.Contains(diagnostics, d => d.Message.Contains("line 3"));
        Assert.Contains(diagnostics, d => d.Message.Contains("line 4"));
        Assert.All(diagnostics, d =>
        {
            Assert.Equal("t", d.Id);
            Assert.NotEqual(d.Message, d.GetMessage("Russian"));
            Assert.NotEqual(d.Message, d.GetMessage("Ukrainian"));
        });
    }

    [Fact]
    public void GeneratesQuotedSqlForEachConstraintWithoutDependingOnTableOrEdgeOrder()
    {
        SchemaTable[] tables = [
            new("root", "select", [new("key", "TEXT", PrimaryKey: true), new("parent", "TEXT")]),
            new("child", "loan \"β\"", [
                new("u", "integer", Nullable: false, Unique: true), new("who", "text", UniqueWhenNull: "BACK"), new("back", "TEXT")])
        ];
        SchemaRelation[] relations = [new("self", "root", "root", "PARENT"), new("borrow", "root", "child", "who")];
        var result = SchemaCompiler.Compile(tables, relations);
        Assert.Empty(result.Errors);
        string sql = Assert.IsType<string>(result.Sql);
        Assert.StartsWith("PRAGMA foreign_keys = ON;", sql);
        Assert.Contains("CREATE TABLE \"loan \"\"β\"\"\" (", sql);
        Assert.Contains("\"key\" TEXT PRIMARY KEY NOT NULL", sql);
        Assert.Contains("\"u\" INTEGER NOT NULL UNIQUE", sql);
        Assert.Contains("FOREIGN KEY (\"parent\") REFERENCES \"select\" (\"key\")", sql);
        Assert.Contains("FOREIGN KEY (\"who\") REFERENCES \"select\" (\"key\")", sql);
        Assert.Contains("ON \"loan \"\"β\"\"\" (\"who\")\n    WHERE \"back\" IS NULL", sql);
        Assert.Equal(sql, SchemaCompiler.Compile(tables.Reverse().ToArray(), relations.Reverse().ToArray()).Sql);
    }

    [Fact]
    public void ValidatesNamesAndTypesBeforeGeneratingSql()
    {
        SchemaTable[] tables = [
            new("t", "SQLITE_reserved", [new("v", "DATE"), new("V", "INTEGER")]),
            new("same", "Accounts", [new("id", "TEXT", PrimaryKey: true), new("other", "TEXT", PrimaryKey: true)]),
            new("same", "accounts", []),
            new("bad", "\0", [new("", "TEXT")])
        ];
        var result = SchemaCompiler.Compile(tables, []);
        Assert.False(result.IsValid);
        Assert.Null(result.Sql);
        Assert.Contains(result.Errors, e => e.Message.Contains("sqlite_"));
        Assert.Contains(result.Errors, e => e.Message.Contains("Duplicate field"));
        Assert.Contains(result.Errors, e => e.Message.Contains("Duplicate table"));
        Assert.Contains(result.Errors, e => e.Message.Contains("INTEGER, REAL"));
        Assert.Contains(result.Errors, e => e.Message.Contains("single primary key"));
        Assert.Contains(result.Errors, e => e.Message.Contains("Add a field"));
        Assert.Contains(result.Errors, e => e.Message.Contains("ID"));
        Assert.Contains(result.Errors, e => e.Id == "bad" && e.Message.Contains("field name"));
        Assert.False(SchemaCompiler.Compile([], []).IsValid);
    }

    [Fact]
    public void ValidatesEndpointsTypesAndAmbiguousForeignKeys()
    {
        SchemaTable[] tables = [new("p", "parent", [new("key", "TEXT", PrimaryKey: true)]),
            new("c", "child", [new("id", "INTEGER"), new("name", "TEXT")])];
        SchemaRelation[] relations = [
            new("broken", "gone", "c", "name"), new("missing-column", "p", "c", "no-column"),
            new("type", "p", "c", "id"), new("first", "p", "c", "name"), new("repeated", "p", "c", "NAME"),
            new("missing-pk", "c", "p", "key")
        ];
        var result = SchemaCompiler.Compile(tables, relations);
        Assert.Null(result.Sql);
        Assert.Equal(new[] { "broken", "missing-column", "missing-pk", "repeated", "type" },
            result.Errors.Select(e => e.Id).Order(StringComparer.Ordinal));
        Assert.True(SchemaCompiler.Compile(tables, []).IsValid);
    }

    [Theory]
    [InlineData("unknown", false)]
    [InlineData("key", false)]
    [InlineData("required", false)]
    [InlineData("self", false)]
    [InlineData("nullable", true)]
    public void ValidatesThePartialIndexCondition(string condition, bool valid)
    {
        var table = new SchemaTable("t", "table", [
            new("key", "INTEGER", PrimaryKey: true), new("self", "TEXT", UniqueWhenNull: condition),
            new("required", "TEXT", Nullable: false), new("nullable", "TEXT")
        ]);
        Assert.Equal(valid, SchemaCompiler.Compile([table], []).IsValid);
    }

    [Fact]
    public void PartialIndexNamesShareTheSqliteTableNamespace()
    {
        SchemaTable[] tables = [
            new("t", "T", [new("f", "TEXT", UniqueWhenNull: "n"), new("n", "NUMERIC")]),
            new("conflict", "UX_T_F_OPEN", [new("α", "REAL"), new("Α", "REAL")])
        ];
        var result = SchemaCompiler.Compile(tables, []);
        Assert.Empty(result.Errors);
        Assert.Contains("CREATE UNIQUE INDEX \"ux_T_f_open_2\"", result.Sql);
    }
}

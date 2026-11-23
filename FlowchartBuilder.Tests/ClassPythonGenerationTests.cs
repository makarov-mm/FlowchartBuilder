using Makarov.FlowchartBuilder.Tools.ClassPython;
using Xunit;

namespace FlowchartBuilder.Tests;

public class ClassPythonGenerationTests
{
    [Fact]
    public void ExportsClassBodiesForwardAssociationsAndBaseBeforeChild()
    {
        ClassDefinition[] classes =
        [
            new("member", "Member", "flags: set[str]\n\naddresses: dict[str, list[str | None]]",
                "borrow(title: str, amount: int) -> bool\nhistory() -> tuple[Book, ...]"),
            new("book", "Book", "title: str", ""),
            new("person", "Person", "name: str", "")
        ];
        ClassRelation[] links =
        [
            new("association", ClassRelationKind.Association, "book", "member", "borrower"),
            new("inheritance", ClassRelationKind.Inheritance, "member", "person", "")
        ];
        var result = ClassPythonGenerator.Generate(classes, links);
        Assert.Empty(result.Issues);
        Assert.Contains("borrower: Member", result.Code);
        Assert.Contains("addresses: dict[str, list[str | None]]", result.Code);
        Assert.Contains("def borrow(self, title: str, amount: int) -> bool:", result.Code);
        Assert.Contains("def history(self) -> tuple[Book, ...]:", result.Code);
        Assert.True(result.Code.IndexOf("class Person:", StringComparison.Ordinal) <
            result.Code.IndexOf("class Member(Person):", StringComparison.Ordinal));
        Assert.Equal(result.Code, ClassPythonGenerator.Generate(classes.Reverse().ToArray(), links).Code);
    }

    [Theory]
    [InlineData("title: get_type()", "")]
    [InlineData("title: str\ncall: __import__('os')", "")]
    [InlineData("x: str = 'text'", "")]
    [InlineData("x: Missing", "")]
    [InlineData("x: dict[str]", "")]
    [InlineData("x: list[int, str]", "")]
    [InlineData("x: tuple[int, ..., str]", "")]
    [InlineData("self: int", "")]
    [InlineData("mro: int", "")]
    [InlineData("", "call(self: str) -> None")]
    [InlineData("", "call(x: str, x: int) -> None")]
    [InlineData("", "call(x: str = value()) -> None")]
    [InlineData("", "call() -> None: pass")]
    [InlineData("", "class(x: str) -> str")]
    public void RejectsInvalidOrExecutableSignatures(string fields, string methods)
    {
        var result = ClassPythonGenerator.Generate([new("item", "Item", fields, methods)], []);
        Assert.Equal("", result.Code);
        var issue = Assert.Single(result.Issues);
        Assert.Equal("item", issue.GlyphId);
        Assert.NotEqual(issue.English, issue.Message("Russian"));
        Assert.NotEqual(issue.Russian, issue.Message("Ukrainian"));
    }

    [Theory]
    [InlineData("class")]
    [InlineData("str")]
    [InlineData("list")]
    [InlineData("_dataclass")]
    [InlineData("NotImplementedError")]
    [InlineData("__name__")]
    [InlineData("Item): pass")]
    public void RejectsReservedOrInvalidClassNames(string name)
    {
        var result = ClassPythonGenerator.Generate([new("bad", name, "", "")], []);
        Assert.Equal("bad", Assert.Single(result.Issues).GlyphId);
        Assert.Empty(result.Code);
    }

    [Fact]
    public void ReportsDuplicateClassMemberAndRoleWithTheirGlyphIds()
    {
        ClassDefinition[] classes =
        [
            new("a", "Item", "tag: str", "tag() -> str"),
            new("b", "Item", "", "")
        ];
        ClassRelation[] relations = [new("edge", ClassRelationKind.Association, "b", "a", "item"),
            new("duplicate", ClassRelationKind.Association, "b", "a", "item")];
        var result = ClassPythonGenerator.Generate(classes, relations);
        Assert.Equal(3, result.Issues.Count);
        Assert.Contains(result.Issues, i => i.GlyphId == "a");
        Assert.Contains(result.Issues, i => i.GlyphId == "b");
        Assert.Contains(result.Issues, i => i.GlyphId == "duplicate");
        Assert.Empty(result.Code);
    }

    [Fact]
    public void RejectsBrokenRelationExtraBaseAndInheritanceCycle()
    {
        ClassDefinition[] classes = [new("a", "A", "", ""), new("b", "B", "", ""), new("c", "C", "", "")];
        ClassRelation[] relations =
        [
            new("ab", ClassRelationKind.Inheritance, "a", "b", ""),
            new("ba", ClassRelationKind.Inheritance, "b", "a", ""),
            new("extra", ClassRelationKind.Inheritance, "a", "c", ""),
            new("missing", ClassRelationKind.Association, "c", "", "target"),
            new("role", ClassRelationKind.Association, "c", "a", "__dict__")
        ];
        var result = ClassPythonGenerator.Generate(classes, relations);
        Assert.Empty(result.Code);
        Assert.Equal(new[] { "ab", "ba", "extra", "missing", "role" },
            result.Issues.Select(i => i.GlyphId).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void AllowsMethodOverrideAndRejectsShadowingAnInheritedMethodWithAField()
    {
        ClassDefinition[] classes = [new("a", "Base", "label: str", "run() -> None"), new("b", "Child", "", "run() -> None")];
        ClassRelation[] links = [new("edge", ClassRelationKind.Inheritance, "b", "a", "")];
        Assert.Empty(ClassPythonGenerator.Generate(classes, links).Issues);
        classes[1] = classes[1] with { Methods = "", Attributes = "run: int" };
        Assert.Equal("b", Assert.Single(ClassPythonGenerator.Generate(classes, links).Issues).GlyphId);
    }
}

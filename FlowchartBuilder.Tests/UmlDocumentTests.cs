using Makarov.FlowchartBuilder;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Glyphs.Uml;
using Makarov.FlowchartBuilder.Tools.ClassPython;
using Makarov.Framework.Core.SettingStorages;
using Xunit;

namespace FlowchartBuilder.Tests;

[Collection("Editor")]
public sealed class UmlDocumentTests
{
    private sealed class NoteGlyph : BlockGlyph;

    private static Document OpenLibrary()
    {
        Core.Initialize(new MemoryStorage());
        return Document.LoadExample(Path.Combine(AppContext.BaseDirectory, "Examples", "Library-classes.mfb"));
    }

    [Fact]
    public void LibraryRelationsReconnectToTheSavedClassInstancesAndRetainTheirPorts()
    {
        var document = OpenLibrary();
        Assert.Null(document.FileName);
        var member = document.DocumentSheet.Glyphs.OfType<ClassGlyph>().Single(g => g.Caption == "Member");
        member.Attributes += "\nactive: bool";
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mfb");
        try
        {
            document.Save(path);
            var loaded = Document.Load(path);
            Assert.False(loaded.HasSkippedProperties);
            var glyphs = loaded.DocumentSheet.Glyphs;
            var classes = glyphs.OfType<ClassGlyph>().ToDictionary(g => g.Caption);
            Assert.Equal(3, classes.Count);
            var savedMember = classes["Member"];
            Assert.Equal(member.Id, savedMember.Id);
            Assert.Equal("member_id: int\nactive: bool", savedMember.Attributes.ReplaceLineEndings("\n"));
            Assert.Equal("can_borrow(active_loans: int) -> bool", savedMember.Methods);

            var inheritance = Assert.Single(glyphs.OfType<InheritanceGlyph>());
            Assert.Same(savedMember, inheritance.Source);
            Assert.Same(classes["Person"], inheritance.Target);
            Assert.Equal((0, 2), (inheritance.SourcePort, inheritance.TargetPort));
            var association = Assert.Single(glyphs.OfType<AssociationGlyph>());
            Assert.Same(classes["Loan"], association.Source);
            Assert.Same(savedMember, association.Target);
            Assert.Equal((3, 1), (association.SourcePort, association.TargetPort));
            Assert.Equal("borrower", association.Caption);

            var result = ClassDiagramReader.Generate(loaded);
            Assert.Empty(result.Issues);
            Assert.Contains("active: bool", result.Code);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void EditingTheLoadedExampleUpdatesBaseClassesFieldTypesAndMethodSignatures()
    {
        var document = OpenLibrary();
        var initial = ClassDiagramReader.Generate(document);
        Assert.Empty(initial.Issues);
        Assert.Contains("class Person:", initial.Code);
        Assert.Contains("class Member(Person):", initial.Code);
        Assert.Contains("class Loan:", initial.Code);
        Assert.Contains("borrower: Member", initial.Code);
        Assert.Contains("def can_borrow(self, active_loans: int) -> bool:", initial.Code);
        Assert.True(initial.Code.IndexOf("class Person:", StringComparison.Ordinal) <
                    initial.Code.IndexOf("class Member(Person):", StringComparison.Ordinal));

        var classes = document.DocumentSheet.Glyphs.OfType<ClassGlyph>().ToDictionary(g => g.Caption);
        classes["Person"].Caption = "Patron";
        classes["Member"].Caption = "LibraryMember";
        classes["Loan"].Methods = "extend(days: int) -> None";
        document.CommitChange();
        var updated = ClassDiagramReader.Generate(document);
        Assert.Empty(updated.Issues);
        Assert.Contains("class LibraryMember(Patron):", updated.Code);
        Assert.Contains("borrower: LibraryMember", updated.Code);
        Assert.Contains("def extend(self, days: int) -> None:", updated.Code);
        Assert.DoesNotContain("def is_overdue", updated.Code);
    }

    [Fact]
    public void MixedDiagramIssuesIdentifyOnlyInvalidRelations()
    {
        var document = OpenLibrary();
        var sheet = document.DocumentSheet;
        var member = sheet.Glyphs.OfType<ClassGlyph>().Single(g => g.Caption == "Member");
        var loan = sheet.Glyphs.OfType<ClassGlyph>().Single(g => g.Caption == "Loan");
        var note = new NoteGlyph { Caption = "Return desk" };
        var annotation = new ConnectorGlyph(loan, 0, note, 2);
        sheet.AddToBack([note, annotation]);
        Assert.Empty(ClassDiagramReader.Generate(document).Issues);

        var association = Assert.Single(sheet.Glyphs.OfType<AssociationGlyph>());
        association.Target = note;
        var untyped = new ConnectorGlyph(member, 1, loan, 3);
        sheet.Add(untyped);
        var issues = ClassDiagramReader.Generate(document).Issues;
        Assert.Contains(issues, issue => issue.GlyphId == association.Id);
        Assert.Contains(issues, issue => issue.GlyphId == untyped.Id);
        Assert.DoesNotContain(issues, issue => issue.GlyphId == annotation.Id || issue.GlyphId == note.Id);
    }
}

namespace Makarov.FlowchartBuilder.Tools.EntityRelationship;

internal static class SandboxExamples
{
    internal const string Tables = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name;";

    internal static string ReadTable(string table) =>
        $"SELECT * FROM {SchemaSql.Quote(table)} LIMIT {SandboxDatabase.DisplayRows + 1};";

    internal const string Seed = """
        INSERT INTO books (id, title) VALUES (1, 'Робинзон Крузо');
        INSERT INTO copies (id, book_id) VALUES (1, 1), (2, 1);
        INSERT INTO readers (id, name) VALUES (1, 'Анна'), (2, 'Олег');
        """;

    internal const string Loans = """
        SELECT loans.id, books.title, copies.id AS copy_id, readers.name,
               loans.issued_on, loans.due_on, loans.returned_on
        FROM loans
        JOIN copies ON copies.id = loans.copy_id
        JOIN books ON books.id = copies.book_id
        JOIN readers ON readers.id = loans.reader_id
        ORDER BY loans.id;
        """;

    internal static string Issue(int copy, int reader) => $"""
        INSERT INTO loans (copy_id, reader_id, issued_on, due_on)
        VALUES ({copy}, {reader}, date('now'), date('now', '+14 days'));

        {Loans}
        """;

    internal const string ReturnFirst = """
        UPDATE loans SET returned_on = date('now')
        WHERE copy_id = 1 AND returned_on IS NULL;

        """ + "\n" + Loans;
}

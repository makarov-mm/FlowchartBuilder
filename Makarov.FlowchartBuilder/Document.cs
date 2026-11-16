using Makarov.FlowchartBuilder.Sheets;

namespace Makarov.FlowchartBuilder;

/// <summary>An open document: a sheet with its glyphs, the file it came from and its undo history.</summary>
public sealed partial class Document
{
    private const string UntitledName = "Document";

    private static int _lastId;

    private string _savedSnapshot;

    public Document() : this(new InfiniteSheet()) { }

    private Document(Sheet sheet)
    {
        DocumentSheet = sheet;
        Id = Interlocked.Increment(ref _lastId);
        History = new DocumentHistory(CreateSnapshot());
        _savedSnapshot = History.Current;
    }

    /// <summary>Identifies the document (and its tab) for the session.</summary>
    public int Id { get; }

    public Sheet DocumentSheet { get; private set; }

    public string? Name { get; set; }

    public string? FileName { get; private set; }

    public bool HasSkippedProperties { get; private set; }

    public DocumentHistory History { get; }

    /// <summary>Whether the document changed since it was created, opened or saved.</summary>
    public bool IsModified => History.Current != _savedSnapshot;

    /// <summary>The file name without extension, or a placeholder for a document that was never saved.</summary>
    public string DisplayName => Name ?? UntitledName;

    /// <summary>Name for tabs and the window title, with an asterisk when there are unsaved changes.</summary>
    public string Caption => DisplayName + (IsModified ? " *" : string.Empty);

    /// <summary>Records the current state as an undo step if it changed since the last one.</summary>
    public bool CommitChange() => History.Commit(CreateSnapshot());

    public bool Undo() => Restore(History.Undo());

    public bool Redo() => Restore(History.Redo());

    private bool Restore(string? snapshot)
    {
        if (snapshot is null)
            return false;

        // Undo edits the diagram while keeping the current view.
        Sheet sheet = ReadSheet(ParseXml(snapshot), DisplayName, out _);
        sheet.ViewOrigin = DocumentSheet.ViewOrigin;
        sheet.ScaleFactor = DocumentSheet.ScaleFactor;
        DocumentSheet = sheet;
        return true;
    }
}

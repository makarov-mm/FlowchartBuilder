using System.Globalization;
using System.Xml;
using Makarov.FlowchartBuilder.Commands;
using Makarov.FlowchartBuilder.Forms;
using Makarov.FlowchartBuilder.Sheets;

namespace Makarov.FlowchartBuilder;

public sealed partial class Core
{
    private int _untitledDocuments;

    public IReadOnlyList<Document> Documents => _documents;

    /// <summary>Name for a new document: "Document 1", "Document 2" and so on.</summary>
    public string CreateUntitledName() => $"Document {++_untitledDocuments}";

    /// <summary>Opens a document file, or switches to it when it is already open; reports failures to the user.</summary>
    public bool TryOpenDocument(string fileName)
    {
        string fullPath = Path.GetFullPath(fileName);
        if (_documents.Find(document => document.FileName is { } open && PathsEqual(open, fullPath)) is { } alreadyOpen)
        {
            SelectDocument(alreadyOpen.Id);
            return true;
        }

        Document document;
        try
        {
            document = Document.Load(fullPath);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or XmlException or InvalidDataException)
        {
            ShowError(e.Message);
            return false;
        }

        AddDocument(document);

        if (document.HasSkippedProperties)
        {
            MessageBox.Show(
                MainWindow,
                "The document was saved by another version of FlowchartBuilder. Properties this version does not support were skipped.",
                CurrentTranslator["Msg_Info"],
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        return true;
    }

    public void AddDocument(Document document)
    {
        _documents.Add(document);
        _currentDocumentIndex = _documents.Count - 1;

        MainWindow.DocumentsTabs.AddTab(document.Caption, document.Id);
        OnCurrentDocumentChanged();
    }

    public void SelectDocument(int documentId)
    {
        int index = _documents.FindIndex(document => document.Id == documentId);
        if (index < 0 || index == _currentDocumentIndex)
            return;

        _currentDocumentIndex = index;
        MainWindow.DocumentsTabs.SelectTab(documentId);
        OnCurrentDocumentChanged();
    }

    /// <summary>Saves the document, asking for a file name when it has none; false when cancelled or failed.</summary>
    public bool SaveDocument(Document document, bool askForFileName = false)
    {
        string? fileName = document.FileName;

        if (askForFileName || fileName is null)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = Settings.Environment.DocumentFilter,
                DefaultExt = Settings.Environment.DocumentExtension,
                FileName = document.FileName ?? document.DisplayName,
            };

            if (dialog.ShowDialog(MainWindow) != DialogResult.OK)
                return false;

            fileName = dialog.FileName;
        }

        try
        {
            document.Save(fileName);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            ShowError(e.Message);
            return false;
        }

        RefreshDocumentState();
        return true;
    }

    /// <summary>Saves every document with unsaved changes; stops and returns false when one is cancelled.</summary>
    public bool SaveAllDocuments()
    {
        foreach (Document document in _documents.Where(document => document.IsModified).ToList())
        {
            if (!SaveDocument(document))
                return false;
        }

        return true;
    }

    /// <summary>Closes the document after offering to save its changes; false when the user cancels.</summary>
    public bool CloseDocument(Document document)
    {
        if (!ConfirmClose(document))
            return false;

        int index = _documents.IndexOf(document);
        if (index < 0)
            return true;

        _documents.RemoveAt(index);
        MainWindow.DocumentsTabs.RemoveTab(document.Id);

        if (index < _currentDocumentIndex || _currentDocumentIndex >= _documents.Count)
            _currentDocumentIndex--;

        if (HasDocuments)
            MainWindow.DocumentsTabs.SelectTab(CurrentDocument.Id);

        OnCurrentDocumentChanged();
        return true;
    }

    /// <summary>Closes all documents; stops at the first one whose closing the user cancels.</summary>
    public bool CloseAllDocuments()
    {
        foreach (Document document in _documents.ToList())
        {
            if (!CloseDocument(document))
                return false;
        }

        return true;
    }

    /// <summary>Offers to save every changed document before the application exits; false when the user cancels.</summary>
    public bool ConfirmCloseAll() => _documents.ToList().All(ConfirmClose);

    /// <summary>Records the current state of the current document as an undo step if it changed.</summary>
    public void CommitChange()
    {
        if (HasDocuments && CurrentDocument.CommitChange())
            RefreshDocumentState();
    }

    public void Undo()
    {
        if (HasDocuments && CurrentDocument.Undo())
            OnCurrentDocumentChanged();
    }

    public void Redo()
    {
        if (HasDocuments && CurrentDocument.Redo())
            OnCurrentDocumentChanged();
    }

    /// <summary>Enables the menu commands that apply to the current document and its selection.</summary>
    public void UpdateCommands()
    {
        bool hasDocument = HasDocuments;

        SetEnabled<SaveCommand>(hasDocument);
        SetEnabled<SaveAsCommand>(hasDocument);
        SetEnabled<SaveAllCommand>(_documents.Any(document => document.IsModified));
        SetEnabled<CloseCommand>(hasDocument);
        SetEnabled<CloseAllCommand>(hasDocument);
        SetEnabled<ExportImageCommand>(hasDocument);
        SetEnabled<PageSetupCommand>(hasDocument);
        SetEnabled<PrintCommand>(hasDocument);
        SetEnabled<UndoCommand>(hasDocument && CurrentDocument.History.CanUndo);
        SetEnabled<RedoCommand>(hasDocument && CurrentDocument.History.CanRedo);

        if (ActiveSheet is { } sheet)
        {
            sheet.UpdateGlyphsCommands();
            sheet.UpdateZoomCommands();
        }
        else
        {
            Sheet.DisableGlyphsCommands();
        }
    }

    private static void SetEnabled<TCommand>(bool enabled)
        where TCommand : Command =>
        Command.GetInstance<TCommand>().Enabled = enabled;

    private static bool PathsEqual(string first, string second) =>
        string.Equals(Path.GetFullPath(first), Path.GetFullPath(second), StringComparison.OrdinalIgnoreCase);

    private bool ConfirmClose(Document document)
    {
        if (!document.IsModified)
            return true;

        // Show the document the question is about.
        SelectDocument(document.Id);

        DialogResult answer = MessageBox.Show(
            MainWindow,
            string.Format(CultureInfo.CurrentCulture, CurrentTranslator["Msg_SaveChanges"], document.DisplayName),
            CurrentTranslator["FlowchartBuilder_Caption"],
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Warning);

        return answer switch
        {
            DialogResult.Yes => SaveDocument(document),
            DialogResult.No => true,
            _ => false,
        };
    }

    private void OnCurrentDocumentChanged()
    {
        if (ActiveSheet is { } sheet)
            sheet.ShowProperties();
        else
            _windows.GetWindow<PropertiesForm>().Grid.SelectedObjects = [];

        RefreshDocumentState();
        Redraw();
    }

    /// <summary>Refreshes tab captions, the window title and the command states after an edit or a save.</summary>
    public void RefreshDocumentState()
    {
        foreach (Document document in _documents)
            MainWindow.DocumentsTabs.SetCaption(document.Id, document.Caption);

        string application = CurrentTranslator["FlowchartBuilder_Caption"];
        MainWindow.Text = HasDocuments ? $"{CurrentDocument.Caption} - {application}" : application;

        UpdateCommands();
    }
}

using Makarov.FlowchartBuilder.Sheets;

namespace Makarov.FlowchartBuilder;

/// <summary>
/// An open flowchart: one sheet with its glyphs. Persistence lives in Document_LoadAndSave.cs.
/// </summary>
public sealed partial class Document(Sheet sheet)
{
    public Sheet DocumentSheet { get; } = sheet;

    /// <summary>Caption of the document tab; the file name without extension for loaded documents.</summary>
    public string? Name { get; set; }

    /// <summary>The file the document was loaded from or last saved to.</summary>
    public string? FileName { get; private set; }

    /// <summary>
    /// Set when the file contained properties this version does not know, i.e. it was written by another version.
    /// </summary>
    public bool HasSkippedProperties { get; private set; }
}

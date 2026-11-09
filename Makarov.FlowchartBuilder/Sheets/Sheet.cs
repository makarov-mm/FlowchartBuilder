using System.Reflection;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.FlowchartBuilder.Commands;
using Makarov.FlowchartBuilder.Forms;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Sheets;

/// <summary>
/// Drawing surface of a document: holds the glyphs (front-most first), their groups and the zoom.
/// Mouse interaction lives in Sheet_Mouse.cs, painting in Sheet_Rendering.cs.
/// </summary>
public abstract partial class Sheet
{
    public const float MinScaleFactor = 0.2f;
    public const float MaxScaleFactor = 2f;
    public const int ThumbnailWidth = 100;
    public const int ThumbnailHeight = 100;

    private const float DefaultScaleFactor = 0.5f;

    private readonly List<AbstractGlyph> _glyphs = [];
    private float _scaleFactor = DefaultScaleFactor;

    /// <summary>Zoom; changing it updates the enabled state of the zoom commands.</summary>
    public float ScaleFactor
    {
        get => _scaleFactor;
        set
        {
            _scaleFactor = Math.Clamp(value, MinScaleFactor, MaxScaleFactor);
            Command.GetInstance<ZoomOutCommand>().Enabled = _scaleFactor > MinScaleFactor;
            Command.GetInstance<ZoomInCommand>().Enabled = _scaleFactor < MaxScaleFactor;
        }
    }

    public string Name =>
        GetType().GetCustomAttribute<SheetNameAttribute>()?.Name ?? Core.Instance.CurrentTranslator["UndefinedSheetName"];

    public string Family =>
        GetType().GetCustomAttribute<SheetFamilyAttribute>()?.Family ?? Core.Instance.CurrentTranslator["UndefinedSheetFamily"];

    /// <summary>All glyphs, front-most first.</summary>
    public IReadOnlyList<AbstractGlyph> Glyphs => _glyphs;

    public IEnumerable<AbstractGlyph> SelectedGlyphs => _glyphs.Where(glyph => glyph.Selected);

    public IEnumerable<BlockGlyph> SelectedBlockGlyphs => SelectedGlyphs.OfType<BlockGlyph>();

    public IEnumerable<AbstractGlyph> FixedGlyphs => _glyphs.Where(glyph => glyph.Fixed);

    public IEnumerable<AbstractGlyph> NonFixedGlyphs => _glyphs.Where(glyph => !glyph.Fixed);

    public bool HasGlyphs => _glyphs.Count > 0;

    public bool HasSelection => _glyphs.Any(glyph => glyph.Selected);

    public bool HasFixedGlyphs => _glyphs.Any(glyph => glyph.Fixed);

    public bool HasNonFixedGlyphs => _glyphs.Any(glyph => !glyph.Fixed);

    /// <summary>Groups of block glyphs that are selected and moved together.</summary>
    public List<List<BlockGlyph>> Groups { get; } = [];

    /// <summary>Picture shown in the "New" dialog; the caller owns the bitmap.</summary>
    public abstract Bitmap Thumbnail { get; }

    /// <summary>Adds the glyph in front of all others.</summary>
    public void Add(AbstractGlyph glyph) => _glyphs.Insert(0, glyph);

    /// <summary>Adds glyphs behind all others, keeping their order (front-most first), as they are stored in a file.</summary>
    public void AddToBack(IEnumerable<AbstractGlyph> glyphs) => _glyphs.AddRange(glyphs);

    public void DeselectAllGlyphs()
    {
        foreach (AbstractGlyph glyph in _glyphs)
            glyph.Selected = false;
    }

    public void DeselectAllGlyphs(IEnumerable<AbstractGlyph> except)
    {
        var keep = except.ToHashSet();
        foreach (AbstractGlyph glyph in _glyphs)
        {
            if (!keep.Contains(glyph))
                glyph.Selected = false;
        }
    }

    public void SelectAllGlyphs()
    {
        foreach (AbstractGlyph glyph in NonFixedGlyphs)
            glyph.Selected = true;
    }

    /// <summary>Deletes the selected glyphs together with the connectors and group memberships that pointed to them.</summary>
    public void DeleteSelectedGlyphs()
    {
        var deleted = SelectedGlyphs.ToHashSet();
        _glyphs.RemoveAll(deleted.Contains);

        foreach (List<BlockGlyph> group in Groups)
            group.RemoveAll(deleted.Contains);

        Groups.RemoveAll(group => group.Count < 2);
        RemoveOrphanConnectors();
    }

    public void RemoveOrphanConnectors() =>
        _glyphs.RemoveAll(glyph => glyph is ConnectorGlyph connector &&
            (connector.Source is null || connector.Target is null ||
             !_glyphs.Contains(connector.Source) || !_glyphs.Contains(connector.Target)));

    /// <summary>Ends any drag in progress on the glyphs.</summary>
    public void UnlockGlyphs()
    {
        foreach (BlockGlyph glyph in _glyphs.OfType<BlockGlyph>())
            glyph.Direction = MoveDirection.None;
    }

    /// <summary>Shows the selected block glyphs in the properties window.</summary>
    public void ShowProperties() =>
        Core.Instance.Windows.GetWindow<PropertiesForm>().Grid.SelectedObjects = [.. SelectedBlockGlyphs];

    /// <summary>Enables the commands that make sense for the current selection.</summary>
    public void UpdateGlyphsCommands()
    {
        var selectedBlocks = SelectedBlockGlyphs.ToList();
        var selectedGroups = selectedBlocks.Select(glyph => glyph.Group).OfType<List<BlockGlyph>>().Distinct().ToList();
        int freeBlocks = selectedBlocks.Count(glyph => !glyph.InGroup);
        bool hasSelection = HasSelection;

        // Grouping changes something only when at least two groups or free glyphs are selected.
        SetEnabled<GroupCommand>(selectedGroups.Count + freeBlocks >= 2);
        SetEnabled<UngroupCommand>(selectedGroups.Count > 0);
        SetEnabled<DeleteCommand>(hasSelection);
        SetEnabled<CutCommand>(hasSelection);
        SetEnabled<CopyCommand>(hasSelection);
        SetEnabled<FixSelectedCommand>(hasSelection);
        SetEnabled<DeselectCommand>(hasSelection);
        SetEnabled<FixAllCommand>(HasGlyphs);
        SetEnabled<UnfixAllCommand>(HasFixedGlyphs);
        SetEnabled<PasteCommand>(Core.Instance.Buffer.Count > 0);
        SetEnabled<ExportImageCommand>(true);
        SetEnabled<SaveCommand>(true);
    }

    private static void SetEnabled<TCommand>(bool enabled)
        where TCommand : Command =>
        Command.GetInstance<TCommand>().Enabled = enabled;
}

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

    private const float DefaultScaleFactor = 1f;

    private readonly List<AbstractGlyph> _glyphs = [];
    private float _scaleFactor = DefaultScaleFactor;

    /// <summary>Zoom; changing it updates the enabled state of the zoom commands.</summary>
    public float ScaleFactor
    {
        get => _scaleFactor;
        set
        {
            _scaleFactor = Math.Clamp(value, MinScaleFactor, MaxScaleFactor);
            UpdateZoomCommands();
        }
    }

    /// <summary>Top-left of the viewport in document pixels at 100% zoom.</summary>
    public PointF ViewOrigin { get; set; }

    public Point ScaledViewOrigin => Point.Round(new PointF(ViewOrigin.X * ScaleFactor, ViewOrigin.Y * ScaleFactor));

    public Rectangle GetContentBounds()
    {
        RectangleF bounds = RectangleF.Empty;
        foreach (BlockGlyph block in Glyphs.OfType<BlockGlyph>())
        {
            var box = block.RoutingBounds(false);
            float edge = Math.Max(1f, UnitsConverter.MMToPxExact(block.BorderWidthInMM)) / 2;
            box.Inflate(edge, edge);
            bounds = bounds.IsEmpty ? box : RectangleF.Union(bounds, box);
        }
        if (bounds.IsEmpty)
            return Rectangle.Empty;

        foreach (var link in Glyphs.OfType<ConnectorGlyph>())
        {
            if (link.Source is not { } source || link.Target is not { } target) continue;
            var route = ConnectorRoute.Create(link.RoutingBetween(source, target),
                source.PortPosition(link.SourcePort, false), source.PortDirection(link.SourcePort), source.RoutingBounds(false),
                target.PortPosition(link.TargetPort, false), target.PortDirection(link.TargetPort), target.RoutingBounds(false),
                Math.Max(8f, UnitsConverter.MMToPxExact(6f)));
            float pad = UnitsConverter.MMToPxExact(link.LineWidthInMM) * 2f;
            foreach (var point in route.Polyline)
                bounds = RectangleF.Union(bounds, new RectangleF(point.X-pad, point.Y-pad, pad*2, pad*2));
            if (!string.IsNullOrWhiteSpace(link.Caption))
            {
                var size = Makarov.Framework.Graphics.Canvas.MeasureText(link.Caption, new("Arial", 8f));
                bounds = RectangleF.Union(bounds, new RectangleF(route.Midpoint.X - size.Width/2,
                    route.Midpoint.Y - size.Height/2, size.Width, size.Height));
            }
        }
        bounds.Inflate(16f, 16f);
        return Rectangle.FromLTRB((int)MathF.Floor(bounds.Left), (int)MathF.Floor(bounds.Top),
                                  (int)MathF.Ceiling(bounds.Right), (int)MathF.Ceiling(bounds.Bottom));
    }

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
    public void ShowProperties()
    {
        var core = Core.Instance;
        core.Windows.GetWindow<PropertiesForm>().Grid.SelectedObjects = [.. SelectedGlyphs];
        core.NotifySelectionChanged();
    }

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
        SetEnabled<CutCommand>(selectedBlocks.Count > 0);
        SetEnabled<CopyCommand>(selectedBlocks.Count > 0);
        SetEnabled<FixSelectedCommand>(hasSelection);
        SetEnabled<DeselectCommand>(hasSelection);
        SetEnabled<FixAllCommand>(HasGlyphs);
        SetEnabled<UnfixAllCommand>(HasFixedGlyphs);
        SetEnabled<PasteCommand>(GlyphClipboard.Available);
    }

    public void UpdateZoomCommands()
    {
        SetEnabled<ZoomOutCommand>(_scaleFactor > MinScaleFactor);
        SetEnabled<ZoomInCommand>(_scaleFactor < MaxScaleFactor);
    }

    /// <summary>Disables the commands that need a sheet, for when no document is open.</summary>
    public static void DisableGlyphsCommands()
    {
        SetEnabled<GroupCommand>(false);
        SetEnabled<UngroupCommand>(false);
        SetEnabled<DeleteCommand>(false);
        SetEnabled<CutCommand>(false);
        SetEnabled<CopyCommand>(false);
        SetEnabled<PasteCommand>(false);
        SetEnabled<FixSelectedCommand>(false);
        SetEnabled<DeselectCommand>(false);
        SetEnabled<FixAllCommand>(false);
        SetEnabled<UnfixAllCommand>(false);
        SetEnabled<ZoomInCommand>(false);
        SetEnabled<ZoomOutCommand>(false);
    }

    private static void SetEnabled<TCommand>(bool enabled)
        where TCommand : Command =>
        Command.GetInstance<TCommand>().Enabled = enabled;
}

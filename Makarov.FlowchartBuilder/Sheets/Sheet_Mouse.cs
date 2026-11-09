using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Sheets;

/// <summary>
/// Mouse interaction in sheet pixel coordinates: placing new glyphs, selecting, moving, resizing,
/// rotating and connecting glyphs.
/// </summary>
public abstract partial class Sheet
{
    private const float ConnectorHitTolerance = 4f;

    private Point _lastMousePosition;
    private Point? _selectionStart;
    private Point _selectionEnd;
    private BlockGlyph? _connectorSource;
    private int _connectorSourcePort;
    private Point? _connectorCurrent;

    public void MouseDown(Point point)
    {
        if (Core.Instance.CurrentGlyphType is { } glyphType && glyphType.IsAssignableTo(typeof(BlockGlyph)))
            PlaceNewGlyph(glyphType, point);

        _selectionStart = null;
        _lastMousePosition = point;
        UnlockGlyphs();

        if (TryStartConnector(point) || TryHitBlock(point) || TrySelectConnector(point))
            return;

        _selectionStart = point;
        _selectionEnd = point;
        DeselectAllGlyphs();
        OnSelectionChanged();
    }

    public void MouseMove(Point point)
    {
        if (_connectorSource is not null)
        {
            _connectorCurrent = point;
            _lastMousePosition = point;
            Core.Instance.Redraw();
            return;
        }

        if (_selectionStart.HasValue)
        {
            _selectionEnd = point;
            Core.Instance.Redraw();
            return;
        }

        bool changed = false;
        foreach (BlockGlyph glyph in NonFixedGlyphs.OfType<BlockGlyph>().Where(glyph => glyph.Direction != MoveDirection.None))
        {
            DragGlyph(glyph, point);
            changed = true;
        }

        _lastMousePosition = point;

        if (changed)
            Core.Instance.Redraw();
    }

    public void MouseUp()
    {
        if (_connectorSource is not null)
        {
            FinishConnector();
        }
        else if (_selectionStart is { } start)
        {
            SelectInRectangle(Normalize(start, _selectionEnd));
            _selectionStart = null;
        }

        Core.Instance.MainWindow.Cursor = Cursors.Arrow;
        UnlockGlyphs();
        OnSelectionChanged();
    }

    private void PlaceNewGlyph(Type glyphType, Point point)
    {
        var glyph = (BlockGlyph)Activator.CreateInstance(glyphType)!;
        Core.Instance.CurrentGlyphType = null;

        glyph.ScaledX = point.X;
        glyph.ScaledY = point.Y;

        DeselectAllGlyphs();
        glyph.Selected = true;
        Add(glyph);
    }

    /// <summary>A press on a link port of an unselected glyph starts dragging a new connector.</summary>
    private bool TryStartConnector(Point point)
    {
        foreach (BlockGlyph glyph in NonFixedGlyphs.OfType<BlockGlyph>())
        {
            if (!glyph.SupportsLinks || glyph.Selected)
                continue;

            int port = HitLinkPoint(glyph, point);
            if (port < 0)
                continue;

            _connectorSource = glyph;
            _connectorSourcePort = port;
            _connectorCurrent = point;

            DeselectAllGlyphs();
            Core.Instance.MainWindow.Cursor = Cursors.Cross;
            OnSelectionChanged();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Front to back: a selected glyph reacts to its resize and rotation handles first, then any glyph to its body.
    /// </summary>
    private bool TryHitBlock(Point point)
    {
        foreach (BlockGlyph glyph in NonFixedGlyphs.OfType<BlockGlyph>().ToList())
        {
            if (glyph.Selected && (TryStartResize(glyph, point) || TryStartRotation(glyph, point)))
                return true;

            if (glyph.Contains(point))
            {
                SelectForMove(glyph);
                return true;
            }
        }

        return false;
    }

    private bool TryStartResize(BlockGlyph glyph, Point point)
    {
        float half = glyph.ManipulationPointSize / 2f;

        foreach (ManipPoint handle in glyph.ManipulationPoints)
        {
            if (!IsNear(point, handle.Coords, half))
                continue;

            glyph.DirectionsAngle = handle.ArrowDirection.AngleTo(handle.MoveDirection);
            glyph.Direction = handle.MoveDirection;
            DeselectAllGlyphs([glyph]);

            Core.Instance.MainWindow.Cursor = handle.ArrowDirection switch
            {
                MoveDirection.Up or MoveDirection.Down => Cursors.SizeNS,
                MoveDirection.Left or MoveDirection.Right => Cursors.SizeWE,
                MoveDirection.DownAndLeft or MoveDirection.UpAndRight => Cursors.SizeNESW,
                _ => Cursors.SizeNWSE,
            };

            OnSelectionChanged();
            return true;
        }

        return false;
    }

    private bool TryStartRotation(BlockGlyph glyph, Point point)
    {
        if (!glyph.CanRotate || !IsNear(point, glyph.RotationPoints[3], glyph.RotationPointSize / 2f))
            return false;

        glyph.Direction = MoveDirection.Rotate;
        DeselectAllGlyphs([glyph]);

        Core.Instance.MainWindow.Cursor = Cursors.Hand;
        OnSelectionChanged();
        return true;
    }

    /// <summary>
    /// Brings the glyph (or its whole group) to the front, selects it and starts moving it together
    /// with everything else that is selected.
    /// </summary>
    private void SelectForMove(BlockGlyph glyph)
    {
        List<BlockGlyph> unit = glyph.Group ?? [glyph];

        if (!glyph.Selected)
        {
            BringToFront(unit);
            DeselectAllGlyphs(unit);
        }

        foreach (BlockGlyph selected in SelectedBlockGlyphs)
            selected.Direction = MoveDirection.Move;

        foreach (BlockGlyph member in unit.Where(member => !member.Fixed))
        {
            member.Selected = true;
            member.Direction = MoveDirection.Move;
        }

        Core.Instance.MainWindow.Cursor = Cursors.SizeAll;
        OnSelectionChanged();
    }

    private bool TrySelectConnector(Point point)
    {
        ConnectorGlyph? connector = NonFixedGlyphs.OfType<ConnectorGlyph>()
            .FirstOrDefault(candidate => candidate.HitTest(point, ConnectorHitTolerance));
        if (connector is null)
            return false;

        DeselectAllGlyphs();
        connector.Selected = true;
        OnSelectionChanged();
        return true;
    }

    private void DragGlyph(BlockGlyph glyph, Point point)
    {
        int dxPixels = point.X - _lastMousePosition.X;
        int dyPixels = point.Y - _lastMousePosition.Y;

        switch (glyph.Direction)
        {
            case MoveDirection.Move:
                glyph.ScaledXInMM += UnitsConverter.PxToMM(dxPixels);
                glyph.ScaledYInMM += UnitsConverter.PxToMM(dyPixels);
                break;

            case MoveDirection.Rotate:
                float angle = float.DegreesToRadians(glyph.Angle);
                glyph.Angle += dxPixels * MathF.Cos(angle) + dyPixels * MathF.Sin(angle);
                break;

            default:
                // The glyph grows symmetrically around its center, hence the doubled movement, which is
                // projected onto the rotated axes of the glyph.
                float radians = float.DegreesToRadians(glyph.DirectionsAngle);
                float cos = MathF.Cos(radians);
                float sin = MathF.Sin(radians);
                float dx = UnitsConverter.PxToMM(dxPixels << 1);
                float dy = UnitsConverter.PxToMM(dyPixels << 1);
                float alongHeight = dx * sin - dy * cos;
                float alongWidth = dx * cos + dy * sin;

                if (glyph.Direction.HasFlag(MoveDirection.Up))
                    glyph.ScaledHeightInMM += alongHeight;
                if (glyph.Direction.HasFlag(MoveDirection.Down))
                    glyph.ScaledHeightInMM -= alongHeight;
                if (glyph.Direction.HasFlag(MoveDirection.Left))
                    glyph.ScaledWidthInMM -= alongWidth;
                if (glyph.Direction.HasFlag(MoveDirection.Right))
                    glyph.ScaledWidthInMM += alongWidth;
                break;
        }
    }

    private void FinishConnector()
    {
        BlockGlyph source = _connectorSource!;

        if (_connectorCurrent is { } release)
        {
            BlockGlyph? target = NonFixedGlyphs.OfType<BlockGlyph>()
                .FirstOrDefault(glyph => glyph.SupportsLinks && !ReferenceEquals(glyph, source) && glyph.Contains(release));

            if (target is not null)
                Add(new ConnectorGlyph(source, _connectorSourcePort, target, NearestLinkPoint(target, release)));
        }

        _connectorSource = null;
        _connectorCurrent = null;
    }

    /// <summary>Selects the block glyphs touching <paramref name="area"/>, extended to whole groups.</summary>
    private void SelectInRectangle(Rectangle area)
    {
        DeselectAllGlyphs();

        foreach (BlockGlyph glyph in NonFixedGlyphs.OfType<BlockGlyph>().Where(glyph => glyph.InRegion(area)))
            glyph.Selected = true;

        var groups = SelectedBlockGlyphs.Select(glyph => glyph.Group).OfType<List<BlockGlyph>>().Distinct().ToList();
        foreach (BlockGlyph member in groups.SelectMany(group => group).Where(member => !member.Fixed))
            member.Selected = true;
    }

    private void BringToFront(List<BlockGlyph> glyphs)
    {
        _glyphs.RemoveAll(glyph => glyph is BlockGlyph block && glyphs.Contains(block));
        _glyphs.InsertRange(0, glyphs);
    }

    private void OnSelectionChanged()
    {
        UpdateGlyphsCommands();
        ShowProperties();
        Core.Instance.Redraw();
    }

    private static bool IsNear(Point point, PointF target, float half) =>
        point.X >= target.X - half && point.X <= target.X + half &&
        point.Y >= target.Y - half && point.Y <= target.Y + half;

    private static int HitLinkPoint(BlockGlyph glyph, Point point)
    {
        PointF[] ports = glyph.LinkPointsScaled;
        float half = glyph.LinkPointSize / 2f + 1f;

        for (int i = 0; i < ports.Length; i++)
        {
            if (IsNear(point, ports[i], half))
                return i;
        }

        return -1;
    }

    private static int NearestLinkPoint(BlockGlyph glyph, Point point)
    {
        PointF[] ports = glyph.LinkPointsScaled;
        int nearest = 0;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < ports.Length; i++)
        {
            float dx = point.X - ports[i].X;
            float dy = point.Y - ports[i].Y;
            float distance = dx * dx + dy * dy;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = i;
            }
        }

        return nearest;
    }
}

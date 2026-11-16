using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Sheets;

public abstract partial class Sheet
{
    private Point _lastMousePosition;
    private Point? _selectionStart;
    private Point _selectionEnd;
    private BlockGlyph? _connectorSource;
    private int _connectorSourcePort;
    private ConnectorGlyph? _connectorPrototype;
    private ConnectorGlyph? _draggedConnector;
    private ConnectorEnd _draggedEnd;
    private BlockGlyph? _hoveredBlock;
    private int _hoveredPort;
    private Point? _connectorCurrent;

    /// <summary>
    /// The block at the fixed end of the connector being created or edited; a connector never links a block to itself.
    /// </summary>
    private BlockGlyph? AnchorBlock => _draggedConnector is { } connector
        ? connector.BlockAt(_draggedEnd == ConnectorEnd.Source ? ConnectorEnd.Target : ConnectorEnd.Source)
        : _connectorSource;

    public void MouseDown(Point point)
    {
        _selectionStart = null;
        _lastMousePosition = point;
        UnlockGlyphs();

        if (Core.Instance.CurrentGlyphType is { } glyphType)
        {
            if (glyphType.IsAssignableTo(typeof(ConnectorGlyph)))
            {
                // A link tool from the palette: drag from any block that has link ports.
                if (FindLinkableBlock(point, except: null) is { } source)
                    StartConnector(source, NearestLinkPoint(source, point), glyphType, point);

                return;
            }

            if (glyphType.IsAssignableTo(typeof(BlockGlyph)))
                if (!PlaceNewGlyph(glyphType, point)) return;
        }

        if (TryStartEndDrag(point) || TryStartConnector(point) || TryHitBlock(point) || TrySelectConnector(point))
            return;

        _selectionStart = point;
        _selectionEnd = point;
        DeselectAllGlyphs();
        OnSelectionChanged();
    }

    public void MouseMove(Point point)
    {
        if (_connectorSource is not null || _draggedConnector is not null)
        {
            _connectorCurrent = point;
            _hoveredBlock = FindLinkableBlock(point, except: AnchorBlock);
            _hoveredPort = _hoveredBlock is null ? 0 : NearestLinkPoint(_hoveredBlock, point);
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
        else if (_draggedConnector is not null)
        {
            FinishEndDrag();
        }
        else if (_selectionStart is { } start)
        {
            SelectInRectangle(Normalize(start, _selectionEnd));
            _selectionStart = null;
        }

        Core.Instance.MainWindow.Cursor = Cursors.Default;
        UnlockGlyphs();
        OnSelectionChanged();

        // Moves, resizes, rotations, new glyphs and connectors become one undo step each.
        Core.Instance.CommitChange();
    }

    private bool PlaceNewGlyph(Type glyphType, Point point)
    {
        var glyph = (BlockGlyph)Activator.CreateInstance(glyphType)!;
        Core.Instance.CurrentGlyphType = null;

        if (!glyph.PreparePlacement(Core.Instance.MainWindow)) return false;
        glyph.ScaledX = point.X;
        glyph.ScaledY = point.Y;

        DeselectAllGlyphs();
        glyph.Selected = true;
        Add(glyph);
        return true;
    }

    /// <summary>Starts a connector when a link port of an unselected block is pressed.</summary>
    private bool TryStartConnector(Point point)
    {
        foreach (BlockGlyph glyph in NonFixedGlyphs.OfType<BlockGlyph>())
        {
            if (!glyph.SupportsLinks || glyph.Selected)
                continue;

            int port = HitLinkPoint(glyph, point);
            if (port < 0)
                continue;

            StartConnector(glyph, port, typeof(ConnectorGlyph), point);
            return true;
        }

        return false;
    }

    private void StartConnector(BlockGlyph source, int port, Type connectorType, Point point)
    {
        _connectorSource = source;
        _connectorSourcePort = port;
        _connectorPrototype = (ConnectorGlyph)Activator.CreateInstance(connectorType)!;
        _hoveredBlock = null;
        _connectorCurrent = point;

        DeselectAllGlyphs();
        Core.Instance.MainWindow.Cursor = Cursors.Cross;
        OnSelectionChanged();
    }

    private void FinishConnector()
    {
        BlockGlyph source = _connectorSource!;
        ConnectorGlyph connector = _connectorPrototype!;

        if (_connectorCurrent is { } release && FindLinkableBlock(release, except: source) is { } target)
        {
            connector.Connect(source, _connectorSourcePort, target, NearestLinkPoint(target, release));
            Add(connector);

            // A link tool picked in the palette is used once, like the block tools.
            if (Core.Instance.CurrentGlyphType?.IsAssignableTo(typeof(ConnectorGlyph)) == true)
                Core.Instance.CurrentGlyphType = null;
        }

        _connectorSource = null;
        _connectorPrototype = null;
        _hoveredBlock = null;
        _connectorCurrent = null;
    }

    /// <summary>Starts moving an end of a selected connector when its handle is pressed.</summary>
    private bool TryStartEndDrag(Point point)
    {
        foreach (ConnectorGlyph connector in SelectedGlyphs.OfType<ConnectorGlyph>())
        {
            ConnectorEnd end = connector.HitEnd(point);

            // A selected block keeps its resize handles, which lie on the ports.
            if (end == ConnectorEnd.None || connector.BlockAt(end) is not { Selected: false })
                continue;

            SnapPointer(ConnectorGlyph.PortLocation(connector.BlockAt(end)!, end == ConnectorEnd.Source ? connector.SourcePort : connector.TargetPort));
            _draggedConnector = connector;
            _draggedEnd = end;
            _hoveredBlock = null;
            _connectorCurrent = point;

            Core.Instance.MainWindow.Cursor = Cursors.Cross;
            Core.Instance.Redraw();
            return true;
        }

        return false;
    }

    /// <summary>Released over a block, the end moves to the nearest port; anywhere else the connector stays as it was.</summary>
    private void FinishEndDrag()
    {
        ConnectorGlyph connector = _draggedConnector!;

        if (_connectorCurrent is { } release && FindLinkableBlock(release, except: AnchorBlock) is { } block)
            connector.Reconnect(_draggedEnd, block, NearestLinkPoint(block, release));

        _draggedConnector = null;
        _draggedEnd = ConnectorEnd.None;
        _hoveredBlock = null;
        _connectorCurrent = null;
    }

    /// <summary>The topmost block under <paramref name="point"/> (or its link ports) that accepts connectors.</summary>
    private BlockGlyph? FindLinkableBlock(Point point, BlockGlyph? except) =>
        NonFixedGlyphs.OfType<BlockGlyph>().FirstOrDefault(glyph =>
            glyph.SupportsLinks && !ReferenceEquals(glyph, except) && (glyph.Contains(point) || HitLinkPoint(glyph, point) >= 0));

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

            SnapPointer(handle.Coords);
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

        SnapPointer(glyph.RotationPoints[3]);
        glyph.Direction = MoveDirection.Rotate;
        DeselectAllGlyphs([glyph]);

        Core.Instance.MainWindow.Cursor = Cursors.Hand;
        OnSelectionChanged();
        return true;
    }

    private void SnapPointer(PointF handle)
    {
        _lastMousePosition = Point.Round(handle);
        var core = Core.Instance;
        var view = core.MainWindow.SheetView;
        if (!view.Capture && Control.MouseButtons != MouseButtons.Left) return;
        var origin = core.ClientToSheet(Point.Empty);
        Cursor.Position = view.PointToScreen(new Point(_lastMousePosition.X - origin.X, _lastMousePosition.Y - origin.Y));
    }

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
        ConnectorGlyph? connector = NonFixedGlyphs.OfType<ConnectorGlyph>().FirstOrDefault(candidate => candidate.HitTest(point));
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
        Core.Instance.UpdateCommands();
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

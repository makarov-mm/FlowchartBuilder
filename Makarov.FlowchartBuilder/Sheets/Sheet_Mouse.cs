using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Sheets
{
    public abstract partial class Sheet
    {

        public void MouseDown(Point pt)
        {

            if (Core.Instance.CurrGlyphTypeSelected && Core.Instance.CurrentGlyphType != null)
            {

                if (Core.Instance.CurrentGlyphType == typeof(BlockGlyph) ||
                    Core.Instance.CurrentGlyphType.IsSubclassOf(typeof(BlockGlyph)))
                {

                    var glyph = (BlockGlyph)Activator.CreateInstance(Core.Instance.CurrentGlyphType);

                    Core.Instance.CurrentGlyphType = null;

                    glyph.ScaledX = pt.X;
                    glyph.ScaledY = pt.Y;

                    DeselectAllGlyphs();

                    glyph.Selected = true;

                    Add(glyph);

                    Core.Instance.Redraw();
                }
                else
                {

                }
            }

            _selectionStart = null;

            _oldPt = pt;

            UnlockGlyphs();

            foreach (var glyph in NonFixedGlyphs)
            {
                var linkable = glyph as BlockGlyph;

                if (linkable == null || !linkable.SupportsLinks || linkable.Selected)
                    continue;

                int port = HitLinkPoint(linkable, pt);

                if (port >= 0)
                {
                    _connectorSource = linkable;
                    _connectorSourcePort = port;
                    _connectorCurrent = pt;

                    DeselectAllGlyphs();

                    Core.Instance.MainWindow.Cursor = Cursors.Cross;

                    Core.Instance.Redraw();
                    return;
                }
            }

            foreach (var nonFixedGlyph in NonFixedGlyphs)
            {

                if (nonFixedGlyph is BlockGlyph)
                {
                    var blockGlyph = (BlockGlyph)nonFixedGlyph;

                    if (blockGlyph.Selected)
                    {
                        float halfManipPointSize = blockGlyph.ManipulationPointSize / 2f;

                        foreach (var mpt in blockGlyph.ManipulationPoints)
                            if (pt.X >= mpt.Coords.X - halfManipPointSize &&
                                pt.X <= mpt.Coords.X + halfManipPointSize &&
                                pt.Y >= mpt.Coords.Y - halfManipPointSize &&
                                pt.Y <= mpt.Coords.Y + halfManipPointSize)
                            {

                                blockGlyph.DirectionsAngle = mpt.ArrowDirection.CalculateAngle(mpt.MoveDirection);
                                blockGlyph.Direction = mpt.MoveDirection;

                                DeselectAllGlyphs(new AbstractGlyph[] { blockGlyph });

                                switch (mpt.ArrowDirection)
                                {
                                    case MoveDirection.Up:
                                    case MoveDirection.Down:
                                        Core.Instance.MainWindow.Cursor = Cursors.SizeNS;
                                        break;

                                    case MoveDirection.Left:
                                    case MoveDirection.Right:
                                        Core.Instance.MainWindow.Cursor = Cursors.SizeWE;
                                        break;

                                    case MoveDirection.DownAndLeft:
                                    case MoveDirection.UpAndRight:
                                        Core.Instance.MainWindow.Cursor = Cursors.SizeNESW;
                                        break;

                                    case MoveDirection.DownAndRight:
                                    case MoveDirection.UpAndLeft:
                                        Core.Instance.MainWindow.Cursor = Cursors.SizeNWSE;
                                        break;
                                }

                                Core.Instance.Redraw();
                                return;
                            }

                        PointF rotPt = blockGlyph.RotationPoints[3];
                        float halfRotationPointSize = blockGlyph.RotationPointSize / 2f;

                        if (blockGlyph.CanRotate &&
                            pt.X >= rotPt.X - halfRotationPointSize &&
                            pt.X <= rotPt.X + halfRotationPointSize &&
                            pt.Y >= rotPt.Y - halfRotationPointSize &&
                            pt.Y <= rotPt.Y + halfRotationPointSize)
                        {

                            blockGlyph.Direction = MoveDirection.Rotate;

                            DeselectAllGlyphs(new AbstractGlyph[] { blockGlyph });

                            Core.Instance.MainWindow.Cursor = Cursors.Hand;

                            Core.Instance.Redraw();
                            return;
                        }
                    }

                    int halfScaledWidth = blockGlyph.ScaledWidth / 2;
                    int haltScaledHeight = blockGlyph.ScaledHeight / 2;

                    if (pt.X >= blockGlyph.ScaledX - halfScaledWidth &&
                        pt.X <= blockGlyph.ScaledX + halfScaledWidth &&
                        pt.Y >= blockGlyph.ScaledY - haltScaledHeight &&
                        pt.Y <= blockGlyph.ScaledY + haltScaledHeight)
                    {

                        if (!blockGlyph.Selected)
                        {
                            if (blockGlyph.InGroup)
                            {

                                foreach (var g in blockGlyph.Group)
                                    _glyphs.Remove(g);

                                foreach (var g in blockGlyph.Group)
                                    _glyphs.Insert(0, g);
                            }
                            else
                            {

                                _glyphs.Remove(nonFixedGlyph);
                                _glyphs.Insert(0, nonFixedGlyph);
                            }
                        }

                        if (blockGlyph.InGroup)
                        {

                            if (blockGlyph.Selected)
                            {

                                foreach (var g in SelectedGlyphs)
                                    if (!g.Fixed && g is BlockGlyph)
                                    {
                                        var bg = (BlockGlyph)g;
                                        bg.Direction = MoveDirection.Move;
                                    }

                                foreach (var g in blockGlyph.Group)
                                {
                                    g.Selected = true;
                                    g.Direction = MoveDirection.Move;
                                }
                            }
                            else
                            {

                                foreach (var g in blockGlyph.Group)
                                {
                                    g.Selected = true;
                                    g.Direction = MoveDirection.Move;
                                }

                                DeselectAllGlyphs(blockGlyph.Group);
                            }
                        }
                        else
                        {

                            if (blockGlyph.Selected)
                            {
                                foreach (var g in SelectedGlyphs)
                                    if (!g.Fixed && g is BlockGlyph)
                                    {
                                        var bg = (BlockGlyph)g;
                                        bg.Direction = MoveDirection.Move;
                                    }
                            }
                            else
                            {

                                blockGlyph.Selected = true;

                                blockGlyph.Direction = MoveDirection.Move;

                                DeselectAllGlyphs(new AbstractGlyph[] { blockGlyph });
                            }
                        }

                        Core.Instance.MainWindow.Cursor = Cursors.SizeAll;

                        ShowProperties();

                        Core.Instance.Redraw();
                        UpdateGlyphsCommands();
                        return;
                    }
                }
            }

            foreach (var glyph in _glyphs)
            {
                var connector = glyph as ConnectorGlyph;

                if (connector != null && connector.HitTest(pt, 4f))
                {
                    DeselectAllGlyphs();

                    connector.Selected = true;

                    UpdateGlyphsCommands();

                    Core.Instance.Redraw();
                    return;
                }
            }

            _selectionStart = pt;

            _selectionEnd = pt;

            DeselectAllGlyphs();

            UpdateGlyphsCommands();

            Core.Instance.Redraw();
        }

        public void MouseUp()
        {

            if (_connectorSource != null)
            {
                BlockGlyph target = null;
                int targetPort = 0;

                if (_connectorCurrent.HasValue)
                {
                    Point releasePt = _connectorCurrent.Value;

                    foreach (var glyph in NonFixedGlyphs)
                    {
                        var linkable = glyph as BlockGlyph;

                        if (linkable == null || !linkable.SupportsLinks || ReferenceEquals(linkable, _connectorSource))
                            continue;

                        if (PointInBlock(linkable, releasePt))
                        {
                            target = linkable;
                            targetPort = NearestLinkPoint(linkable, releasePt);
                            break;
                        }
                    }
                }

                if (target != null)
                    Add(new ConnectorGlyph(_connectorSource, _connectorSourcePort, target, targetPort));

                _connectorSource = null;
                _connectorCurrent = null;

                Core.Instance.MainWindow.Cursor = Cursors.Arrow;

                UpdateGlyphsCommands();

                Core.Instance.Redraw();
                return;
            }

            if (SelectionExists)
            {

                DeselectAllGlyphs();

                int x = _selectionStart.Value.X > _selectionEnd.X
                            ? _selectionEnd.X
                            : _selectionStart.Value.X;
                int y = _selectionStart.Value.Y > _selectionEnd.Y
                            ? _selectionEnd.Y
                            : _selectionStart.Value.Y;

                int width = Math.Abs(_selectionEnd.X - _selectionStart.Value.X);
                int height = Math.Abs(_selectionEnd.Y - _selectionStart.Value.Y);

                foreach (var glyph in NonFixedGlyphs)
                    if (glyph is BlockGlyph)
                    {
                        var vg = (BlockGlyph)glyph;
                        if (vg.InRegion(new Rectangle(x, y, width, height)))
                            vg.Selected = true;
                    }

                var addGlyphs = new List<BlockGlyph>();
                foreach (var glyph in SelectedGlyphs)
                    if (glyph is BlockGlyph && !glyph.Fixed)
                    {
                        var bg = (BlockGlyph)glyph;
                        if (bg.InGroup)
                        {
                            foreach (var glyphInGroup in bg.Group)
                                if (!addGlyphs.Contains(glyphInGroup))
                                {
                                    addGlyphs.Add(glyphInGroup);
                                    glyphInGroup.Selected = true;
                                }
                        }
                    }
            }

            _selectionStart = null;
            Core.Instance.MainWindow.Cursor = Cursors.Arrow;

            UnlockGlyphs();

            UpdateGlyphsCommands();

            Core.Instance.Redraw();
        }

        public void MouseMove(Point pt)
        {

            if (_connectorSource != null)
            {
                _connectorCurrent = pt;
                _oldPt = pt;
                Core.Instance.Redraw();
                return;
            }

            if (_selectionStart.HasValue)
            {
                _selectionEnd = pt;
                Core.Instance.Redraw();
                return;
            }

            bool needRedraw = false;

            foreach (AbstractGlyph glyph in NonFixedGlyphs)
                if (glyph is BlockGlyph)
                {
                    var vg = (BlockGlyph)glyph;

                    if (vg.Direction == MoveDirection.Move)
                    {
                        vg.ScaledXInMM += UnitsConverter.PxToMM(pt.X - _oldPt.X);
                        vg.ScaledYInMM += UnitsConverter.PxToMM(pt.Y - _oldPt.Y);
                        needRedraw = true;
                    }
                    else if (vg.Direction != MoveDirection.None)
                    {
                        float angle = vg.DirectionsAngle;
                        float radians = Framework.Core.Math.DegToRad(angle);
                        var cos = (float) Math.Cos(radians);
                        var sin = (float) Math.Sin(radians);

                        switch (vg.Direction)
                        {
                            case MoveDirection.Up:
                                vg.ScaledHeightInMM +=
                                    UnitsConverter.PxToMM((pt.X - _oldPt.X) << 1) * sin -
                                    UnitsConverter.PxToMM((pt.Y - _oldPt.Y) << 1) * cos;
                                break;

                            case MoveDirection.Down:
                                vg.ScaledHeightInMM -=
                                    UnitsConverter.PxToMM((pt.X - _oldPt.X) << 1) * sin -
                                    UnitsConverter.PxToMM((pt.Y - _oldPt.Y) << 1) * cos;
                                break;

                            case MoveDirection.Left:
                                vg.ScaledWidthInMM -=
                                    UnitsConverter.PxToMM((pt.X - _oldPt.X) << 1) * cos +
                                    UnitsConverter.PxToMM((pt.Y - _oldPt.Y) << 1) * sin;
                                break;

                            case MoveDirection.Right:
                                vg.ScaledWidthInMM +=
                                    UnitsConverter.PxToMM((pt.X - _oldPt.X) << 1) * cos +
                                    UnitsConverter.PxToMM((pt.Y - _oldPt.Y) << 1) * sin;
                                break;

                            case MoveDirection.DownAndLeft:
                                vg.ScaledHeightInMM -=
                                    UnitsConverter.PxToMM((pt.X - _oldPt.X) << 1) * sin -
                                    UnitsConverter.PxToMM((pt.Y - _oldPt.Y) << 1) * cos;

                                vg.ScaledWidthInMM -=
                                    UnitsConverter.PxToMM((pt.X - _oldPt.X) << 1) * cos +
                                    UnitsConverter.PxToMM((pt.Y - _oldPt.Y) << 1) * sin;
                                break;

                            case MoveDirection.UpAndRight:
                                vg.ScaledHeightInMM +=
                                    UnitsConverter.PxToMM((pt.X - _oldPt.X) << 1) * sin -
                                    UnitsConverter.PxToMM((pt.Y - _oldPt.Y) << 1) * cos;

                                vg.ScaledWidthInMM +=
                                    UnitsConverter.PxToMM((pt.X - _oldPt.X) << 1) * cos +
                                    UnitsConverter.PxToMM((pt.Y - _oldPt.Y) << 1) * sin;
                                break;

                            case MoveDirection.DownAndRight:
                                vg.ScaledHeightInMM -=
                                    UnitsConverter.PxToMM((pt.X - _oldPt.X) << 1) * sin -
                                    UnitsConverter.PxToMM((pt.Y - _oldPt.Y) << 1) * cos;

                                vg.ScaledWidthInMM +=
                                    UnitsConverter.PxToMM((pt.X - _oldPt.X) << 1) * cos +
                                    UnitsConverter.PxToMM((pt.Y - _oldPt.Y) << 1) * sin;
                                break;

                            case MoveDirection.UpAndLeft:
                                vg.ScaledHeightInMM +=
                                    UnitsConverter.PxToMM((pt.X - _oldPt.X) << 1) * sin -
                                    UnitsConverter.PxToMM((pt.Y - _oldPt.Y) << 1) * cos;

                                vg.ScaledWidthInMM -=
                                    UnitsConverter.PxToMM((pt.X - _oldPt.X) << 1) * cos +
                                    UnitsConverter.PxToMM((pt.Y - _oldPt.Y) << 1) * sin;
                                break;

                            case MoveDirection.Rotate:
                                vg.Angle += (float)(
                                    (pt.X - _oldPt.X) * Math.Cos(Framework.Core.Math.DegToRad(vg.Angle)) +
                                    (pt.Y - _oldPt.Y) * Math.Sin(Framework.Core.Math.DegToRad(vg.Angle)));
                                break;
                        }

                        needRedraw = true;
                    }
                }
                else
                {

                }

            _oldPt = pt;

            if (needRedraw)
                Core.Instance.Redraw();
        }

        private static int HitLinkPoint(BlockGlyph glyph, Point pt)
        {
            PointF[] points = glyph.LinkPointsScaled;
            float half = glyph.LinkPointSize / 2f + 1f;

            for (int i = 0; i < points.Length; i++)
                if (pt.X >= points[i].X - half &&
                    pt.X <= points[i].X + half &&
                    pt.Y >= points[i].Y - half &&
                    pt.Y <= points[i].Y + half)
                    return i;

            return -1;
        }

        private static bool PointInBlock(BlockGlyph glyph, Point pt)
        {
            int halfWidth = glyph.ScaledWidth >> 1;
            int halfHeight = glyph.ScaledHeight >> 1;

            return pt.X >= glyph.ScaledX - halfWidth &&
                   pt.X <= glyph.ScaledX + halfWidth &&
                   pt.Y >= glyph.ScaledY - halfHeight &&
                   pt.Y <= glyph.ScaledY + halfHeight;
        }

        private static int NearestLinkPoint(BlockGlyph glyph, Point pt)
        {
            PointF[] points = glyph.LinkPointsScaled;
            int best = 0;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < points.Length; i++)
            {
                float dx = pt.X - points[i].X;
                float dy = pt.Y - points[i].Y;
                float distance = dx * dx + dy * dy;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            return best;
        }
    }
}

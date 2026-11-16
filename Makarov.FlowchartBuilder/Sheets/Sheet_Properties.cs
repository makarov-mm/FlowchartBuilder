using System;
using System.Collections.Generic;
using System.Drawing;
using Makarov.FlowchartBuilder.Commands;
using Makarov.FlowchartBuilder.Glyphs;
using System.Linq;

namespace Makarov.FlowchartBuilder.Sheets
{

    public abstract partial class Sheet
    {

        public float ScaleFactor
        {
            get { return _scaleFactor; }
            set
            {
                if (value <= MinScaleFactor)
                {
                    value = MinScaleFactor;
                    Command.GetInstance<ZoomOutCommand>().Enabled = false;
                }
                else
                    Command.GetInstance<ZoomOutCommand>().Enabled = true;

                if (value >= MaxScaleFactor)
                {
                    value = MaxScaleFactor;
                    Command.GetInstance<ZoomInCommand>().Enabled = false;
                }
                else
                    Command.GetInstance<ZoomInCommand>().Enabled = true;

                _scaleFactor = value;
            }
        }

        public static float MaxScaleFactor
        {
            get { return 2.0f; }
        }

        public static float MinScaleFactor
        {
            get { return 0.2f; }
        }

        public IEnumerable<AbstractGlyph> Glyphs
        {
            get { return _glyphs; }
        }

        public int GlyphsCount
        {
            get { return _glyphs.Count; }
        }

        public bool GlyphsExists
        {
            get
            {
                return GlyphsCount > 0;
            }
        }

        public IEnumerable<AbstractGlyph> SelectedGlyphs
        {
            get { return _glyphs.Where(glyph => glyph.Selected); }
        }

        public IEnumerable<BlockGlyph> SelectedBlockGlyphs
        {
            get { return SelectedGlyphs.OfType<BlockGlyph>(); }
        }

        public int SelectedGlyphsCount
        {
            get { return _glyphs.Count(glyph => glyph.Selected); }
        }

        public int SelectedBlockGlyphsCount
        {
            get { return _glyphs.Count(glyph => glyph.Selected && glyph is BlockGlyph); }
        }

        public bool SelectedGlyphsExists
        {
            get { return _glyphs.Any(glyph => glyph.Selected); }
        }

        public IEnumerable<AbstractGlyph> NonSelectedGlyphs
        {
            get { return _glyphs.Where(glyph => !glyph.Selected); }
        }

        public int NonSelectedGlyphsCount
        {
            get { return _glyphs.Count(glyph => !glyph.Selected); }
        }

        public bool NonSelectedGlyphsExists
        {
            get { return _glyphs.Any(glyph => !glyph.Selected); }
        }

        public IEnumerable<AbstractGlyph> FixedGlyphs
        {
            get { return _glyphs.Where(glyph => glyph.Fixed); }
        }

        public int FixedGlyphsCount
        {
            get { return _glyphs.Count(glyph => glyph.Fixed); }
        }

        public bool FixedGlyphsExists
        {
            get { return _glyphs.Any(glyph => glyph.Fixed); }
        }

        public IEnumerable<AbstractGlyph> NonFixedGlyphs
        {
            get { return _glyphs.Where(glyph => !glyph.Fixed); }
        }

        public int NonFixedGlyphsCount
        {
            get { return _glyphs.Count(glyph => !glyph.Fixed); }
        }

        public bool NonFixedGlyphsExists
        {
            get { return _glyphs.Any(glyph => !glyph.Fixed); }
        }

        public IEnumerable<AbstractGlyph> LockedGlyphs
        {
            get { return _glyphs.Where(glyph => !(glyph is BlockGlyph && ((BlockGlyph)glyph).Direction == MoveDirection.None)); }
        }

        public int LockedGlyphsCount
        {
            get { return _glyphs.Count(glyph => !(glyph is BlockGlyph && ((BlockGlyph)glyph).Direction == MoveDirection.None)); }
        }

        public bool LockedGlyphsExists
        {
            get { return _glyphs.Any(glyph => !(glyph is BlockGlyph && ((BlockGlyph)glyph).Direction == MoveDirection.None)); }
        }

        public IEnumerable<AbstractGlyph> NonLockedGlyphs
        {
            get { return _glyphs.Where(glyph => glyph is BlockGlyph && ((BlockGlyph)glyph).Direction == MoveDirection.None); }
        }

        public int NonLockedGlyphsCount
        {
            get { return _glyphs.Count(glyph => glyph is BlockGlyph && ((BlockGlyph)glyph).Direction == MoveDirection.None); }
        }

        public bool NonLockedGlyphsExists
        {
            get { return _glyphs.Any(glyph => glyph is BlockGlyph && ((BlockGlyph)glyph).Direction == MoveDirection.None); }
        }

        public Point SelectionStart
        {
            get { return _selectionStart.Value; }
        }

        public Point SelectionEnd
        {
            get { return _selectionEnd; }
        }

        public Size SelectionSize
        {
            get
            {
                return new Size(
                    Math.Abs(SelectionEnd.X - SelectionStart.X),
                    Math.Abs(SelectionEnd.Y - SelectionStart.Y));
            }
        }

        public bool SelectionExists
        {
            get { return _selectionStart.HasValue; }
        }

        public Rectangle Selection
        {
            get { return new Rectangle(SelectionStart, SelectionSize); }
        }

        // Top-left of the viewport, in document pixels at 100% zoom.
        public PointF ViewOrigin { get; set; }

        public Point ScaledViewOrigin => Point.Round(new PointF(
            ViewOrigin.X * ScaleFactor, ViewOrigin.Y * ScaleFactor));

        public RectangleF GetContentBounds()
        {
            RectangleF bounds = RectangleF.Empty;
            foreach (var block in Glyphs.OfType<BlockGlyph>())
            {
                double angle = block.Angle * Math.PI / 180;
                float cos = (float)Math.Abs(Math.Cos(angle));
                float sin = (float)Math.Abs(Math.Sin(angle));
                float width = block.Width * cos + block.Height * sin;
                float height = block.Width * sin + block.Height * cos;
                var box = new RectangleF(block.X - width / 2, block.Y - height / 2, width, height);
                bounds = bounds.IsEmpty ? box : RectangleF.Union(bounds, box);
            }
            return bounds;
        }

        public List<List<BlockGlyph>> Groups
        {
            get;
            private set;
        }
    }
}

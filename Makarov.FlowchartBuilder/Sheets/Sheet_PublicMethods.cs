using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Makarov.FlowchartBuilder.Forms;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Sheets
{
    public abstract partial class Sheet
    {

        public Bitmap Draw()
        {
            return Draw(true);
        }

        public Bitmap DrawUnscaled()
        {
            return Draw(false);
        }

        public abstract Bitmap Draw(bool scaled);

        public abstract object Clone();

        public void Clear()
        {
            _glyphs.Clear();
        }

        public void Add(AbstractGlyph glyph)
        {
            _glyphs.Insert(0, glyph);
        }

        public void DeselectAllGlyphs()
        {
            foreach (AbstractGlyph glyph in _glyphs)
                glyph.Selected = false;
        }

        public void DeselectAllGlyphs(IEnumerable<AbstractGlyph> except)
        {
            foreach (AbstractGlyph glyph in _glyphs)
                if (glyph is BlockGlyph)
                {
                    bool deselect = true;

                    foreach (AbstractGlyph g in except)
                        if (g == glyph)
                        {
                            deselect = false;
                            break;
                        }

                    if (deselect)
                        glyph.Selected = false;
                }
        }

        public void DeselectAllGlyphs(IEnumerable<BlockGlyph> except)
        {
            foreach (AbstractGlyph glyph in _glyphs)
                if (glyph is BlockGlyph)
                {
                    bool deselect = true;

                    foreach (BlockGlyph g in except)
                        if (g == glyph)
                        {
                            deselect = false;
                            break;
                        }

                    if (deselect)
                        glyph.Selected = false;
                }
        }

        public void SelectAllGlyphs()
        {
            foreach (AbstractGlyph glyph in NonFixedGlyphs)
                glyph.Selected = true;
        }

        public void DeleteSelectedGlyphs()
        {
            var toDel = _glyphs.Where(glyph => glyph.Selected).ToList();

            foreach (AbstractGlyph glyph in toDel)
                _glyphs.Remove(glyph);

            RemoveOrphanConnectors();
        }

        public void RemoveOrphanConnectors()
        {
            var orphans = _glyphs
                .OfType<ConnectorGlyph>()
                .Where(connector => !connector.IsResolved ||
                                    !_glyphs.Contains(connector.Source) ||
                                    !_glyphs.Contains(connector.Target))
                .Cast<AbstractGlyph>()
                .ToList();

            foreach (AbstractGlyph connector in orphans)
                _glyphs.Remove(connector);
        }

        public void UnlockGlyphs()
        {
            foreach (AbstractGlyph glyph in _glyphs)
                if (glyph is BlockGlyph)
                    ((BlockGlyph)glyph).Direction = MoveDirection.None;
        }

        public void ShowProperties()
        {

            var gridView = Core.Instance.Windows.GetWindow<PropertiesForm>().Grid;
            gridView.SelectedObjects = SelectedBlockGlyphs.Select(glyph => glyph.GetProxy()).ToArray();
        }
    }
}

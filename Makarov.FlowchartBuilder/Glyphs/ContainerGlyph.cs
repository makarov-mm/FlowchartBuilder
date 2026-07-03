using System.Collections.Generic;

namespace Makarov.FlowchartBuilder.Glyphs
{

    public class ContainerGlyph : BlockGlyph
    {
        #region Constructors

        public ContainerGlyph(int x, int y, int width, int height)
            : base(x, y, width, height)
        { }

        public ContainerGlyph()
        { }
        #endregion

        #region Private members

        private readonly List<AbstractGlyph> _innerGlyphs = new List<AbstractGlyph>();
        #endregion

        #region Public methods

        public void Clear()
        {
            _innerGlyphs.Clear();
        }

        public void Add(AbstractGlyph glyph)
        {
            _innerGlyphs.Add(glyph);
        }

        public void Add(IEnumerable<AbstractGlyph> glyphs)
        {
            foreach (var glyph in glyphs)
                Add(glyph);
        }
        #endregion
    }
}

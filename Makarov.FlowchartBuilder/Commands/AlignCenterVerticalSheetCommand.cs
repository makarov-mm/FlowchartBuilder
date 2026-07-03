using System.Collections.Generic;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class AlignCenterVerticalSheetCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            if (!Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphsExists)
                throw new InvalidContextException(@"Selected glyphs not exists.");

            int bottom = 0;

            var sizedSheet = (ISize)Core.Instance.CurrentDocument.DocumentSheet;
            int top = sizedSheet.Height.MMToPx();

            var lst = new List<BlockGlyph>();

            foreach (var glyph in Core.Instance.CurrentDocument.DocumentSheet.NonFixedGlyphs)
            {

                if (glyph is BlockGlyph && glyph.Selected)
                {

                    var g = (BlockGlyph)glyph;

                    if (g.Y + (g.Height >> 1) > bottom)
                        bottom = g.Y + (g.Height >> 1);

                    if (g.Y - (g.Height >> 1) < top)
                        top = g.Y - (g.Height >> 1);

                    lst.Add(g);
                }
            }

            bottom = sizedSheet.Height.MMToPx() - bottom;
            var offset = (bottom - top) >> 1;
            foreach (var glyph in lst)
                glyph.Y += offset;

            Core.Instance.Redraw();
        }
    }
}

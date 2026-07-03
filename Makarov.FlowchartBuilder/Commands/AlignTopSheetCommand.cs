using System.Collections.Generic;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class AlignTopSheetCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            if (!Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphsExists)
                throw new InvalidContextException(@"Selected glyphs not exists.");

            var sizedSheet = (ISize)Core.Instance.CurrentDocument.DocumentSheet;
            int top = sizedSheet.Height.MMToPx();

            var lst = new List<BlockGlyph>();

            foreach (var glyph in Core.Instance.CurrentDocument.DocumentSheet.NonFixedGlyphs)
            {

                if (glyph is BlockGlyph && glyph.Selected)
                {

                    var g = (BlockGlyph) glyph;

                    if (g.Y - (g.Height >> 1) < top)
                        top = g.Y - (g.Height >> 1);

                    lst.Add(g);
                }
            }

            foreach (var glyph in lst)
                glyph.Y -= top;

            Core.Instance.Redraw();
        }
    }
}

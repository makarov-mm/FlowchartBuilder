using System.Collections.Generic;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class AlignBottomCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            if (!Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphsExists)
                throw new InvalidContextException(@"Selected glyphs not exists.");

            int bottom = int.MinValue;

            var lst = new List<BlockGlyph>();

            var groups = new List<List<BlockGlyph>>();

            foreach (var glyph in Core.Instance.CurrentDocument.DocumentSheet.NonFixedGlyphs)
            {

                if (glyph is BlockGlyph && glyph.Selected)
                {

                    var g = (BlockGlyph)glyph;

                    if (g.InGroup)
                    {

                        if (!groups.Contains(g.Group))
                        {
                            groups.Add(g.Group);

                            if (g.GroupY.MMToPx() + (g.GroupHeight.MMToPx() >> 1) > bottom)
                                bottom = g.GroupY.MMToPx() + (g.GroupHeight.MMToPx() >> 1);
                        }
                    }
                    else
                    {

                        if (g.Y + (g.Height >> 1) > bottom)
                            bottom = g.Y + (g.Height >> 1);

                        lst.Add(g);
                    }
                }
            }

            foreach (var glyph in lst)
                glyph.Y = bottom - (glyph.Height >> 1);

            foreach (var group in groups)
                group[0].GroupY = (bottom - (group[0].GroupHeight.MMToPx() >> 1)).PxToMM();

            Core.Instance.Redraw();
        }
    }
}

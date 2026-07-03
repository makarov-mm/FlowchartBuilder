using System.Collections.Generic;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class AlignRightCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            if (!Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphsExists)
                throw new InvalidContextException(@"Selected glyphs not exists.");

            int right = 0;

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

                            if (g.GroupX.MMToPx() + (g.GroupWidth.MMToPx() >> 1) > right)
                                right = g.GroupX.MMToPx() + (g.GroupWidth.MMToPx() >> 1);
                        }
                    }
                    else
                    {

                        if (g.X + (g.Width >> 1) > right)
                            right = g.X + (g.Width >> 1);

                        lst.Add(g);
                    }
                }
            }

            foreach (var glyph in lst)
                glyph.X = right - (glyph.Width >> 1);

            foreach (var group in groups)
                group[0].GroupX = (right - (group[0].GroupWidth.MMToPx() >> 1)).PxToMM();

            Core.Instance.Redraw();
        }
    }
}

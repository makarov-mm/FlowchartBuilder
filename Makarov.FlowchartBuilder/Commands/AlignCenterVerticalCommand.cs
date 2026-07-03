using System.Collections.Generic;
using Makarov.FlowchartBuilder.Extensions;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class AlignCenterVerticalCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            if (!Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphsExists)
                throw new InvalidContextException(@"Selected glyphs not exists.");

            int center = 0;

            var lst = new List<BlockGlyph>();

            var groups = new List<List<BlockGlyph>>();

            int gCount = 0;

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

                            foreach (var gg in g.Group)
                                center += gg.Y;

                            gCount += g.Group.Count;
                        }
                    }
                    else
                    {

                        center += g.Y;

                        lst.Add(g);
                    }
                }
            }

            center /= lst.Count + gCount;

            foreach (var glyph in lst)
                glyph.Y = center;

            foreach (var group in groups)
                group[0].GroupY = center.PxToMM();

            Core.Instance.Redraw();
        }
    }
}

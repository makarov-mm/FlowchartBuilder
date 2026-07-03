using System.Collections.Generic;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class PasteCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            if (Core.Instance.Buffer.Count == 0)
                throw new InvalidContextException();

            Enabled = false;

            var lst = new List<AbstractGlyph>();

            Core.Instance.CurrentDocument.DocumentSheet.DeselectAllGlyphs();
            foreach (var glyph in Core.Instance.Buffer)
            {
                glyph.Selected = true;
                Core.Instance.CurrentDocument.DocumentSheet.Add(glyph);
                lst.Add(glyph);
            }

            Core.Instance.CurrentDocument.DocumentSheet.DeselectAllGlyphs(lst);
            foreach (var glyph in lst)
                if (!glyph.Selected)
                    glyph.Selected = true;

            Core.Instance.Buffer.Clear();

            Core.Instance.CurrentDocument.DocumentSheet.UpdateGlyphsCommands();

            Core.Instance.Redraw();
        }
    }
}

using System.Collections.Generic;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class GroupCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            if (!Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphsExists)
                throw new InvalidContextException(@"Selected glyphs not exists.");

            var selectedGlyphs = new List<BlockGlyph>();
            foreach (var glyph in Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphs)
                if (glyph is BlockGlyph)
                    selectedGlyphs.Add((BlockGlyph)glyph);

            if (selectedGlyphs.Count <= 1)
                throw new InvalidContextException();

            var newGroups = new List<List<BlockGlyph>>();
            foreach (var group in Core.Instance.CurrentDocument.DocumentSheet.Groups)
            {
                var newGroup = new List<BlockGlyph>();

                foreach (var glyph in group)
                    if (!selectedGlyphs.Contains(glyph) && !newGroup.Contains(glyph))
                        newGroup.Add(glyph);

                newGroups.Add(newGroup);
            }

            Core.Instance.CurrentDocument.DocumentSheet.Groups.Clear();
            foreach (var group in newGroups)
                if (group.Count > 0)
                    Core.Instance.CurrentDocument.DocumentSheet.Groups.Add(group);

            var currGroup = new List<BlockGlyph>();
            foreach (var glyph in selectedGlyphs)
                currGroup.Add(glyph);

            Core.Instance.CurrentDocument.DocumentSheet.Groups.Add(currGroup);

            Enabled = false;

            Core.Instance.CurrentDocument.DocumentSheet.UpdateGlyphsCommands();

            Core.Instance.Redraw();
        }
    }
}

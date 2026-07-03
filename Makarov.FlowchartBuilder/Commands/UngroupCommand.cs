using System.Collections.Generic;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class UngroupCommand : Command
    {

        public override void Run()
        {

            var selectedGlyphs = new List<BlockGlyph>();
            foreach (var glyph in Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphs)
                if (glyph is BlockGlyph)
                    selectedGlyphs.Add((BlockGlyph)glyph);

            var selectedGroups = new List<List<BlockGlyph>>();

            foreach (var glyph in selectedGlyphs)
                foreach (var group in Core.Instance.CurrentDocument.DocumentSheet.Groups)
                    foreach (var g in group)
                        if (g == glyph)
                        {
                            selectedGroups.Add(group);
                            continue;
                        }

            foreach (var group in selectedGroups)
                Core.Instance.CurrentDocument.DocumentSheet.Groups.Remove(group);

            Enabled = false;

            Core.Instance.CurrentDocument.DocumentSheet.UpdateGlyphsCommands();

            Core.Instance.Redraw();
        }
    }
}

using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class CopyCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            if (!Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphsExists)
                throw new InvalidContextException(@"Selected glyphs not exists.");

            Core.Instance.Buffer.Clear();
            foreach (var glyph in Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphs)
                Core.Instance.Buffer.Add((AbstractGlyph)glyph.Clone());

            if (Core.Instance.Buffer.Count == 0)
                throw new InvalidContextException();

            Core.Instance.CurrentDocument.DocumentSheet.UpdateGlyphsCommands();

            GetInstance("PasteCommand").Enabled = true;
        }
    }
}

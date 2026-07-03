namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class FixSelectedCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            if (!Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphsExists)
                throw new InvalidContextException(@"Selected glyphs not exists.");

            foreach (var glyph in Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphs)
                glyph.Fixed = true;

            Core.Instance.CurrentDocument.DocumentSheet.UpdateGlyphsCommands();

            Core.Instance.Redraw();
        }
    }
}

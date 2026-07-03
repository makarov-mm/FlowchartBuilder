namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class UnfixAllCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            if (!Core.Instance.CurrentDocument.DocumentSheet.FixedGlyphsExists)
                throw new InvalidContextException();

            foreach (var glyph in Core.Instance.CurrentDocument.DocumentSheet.FixedGlyphs)
                glyph.Fixed = false;

            Core.Instance.CurrentDocument.DocumentSheet.UpdateGlyphsCommands();

            Core.Instance.Redraw();
        }
    }
}

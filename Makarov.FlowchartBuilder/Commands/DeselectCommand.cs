namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class DeselectCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            if (!Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphsExists)
                throw new InvalidContextException(@"Selected glyphs not exists.");

            Core.Instance.CurrentDocument.DocumentSheet.UnlockGlyphs();
            Core.Instance.CurrentDocument.DocumentSheet.DeselectAllGlyphs();

            Core.Instance.CurrentDocument.DocumentSheet.UpdateGlyphsCommands();

            Core.Instance.Redraw();
        }
    }
}

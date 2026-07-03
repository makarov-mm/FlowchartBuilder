namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class SelectAllCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new Core.CurrentDocumentNotExistsException();

            if (!Core.Instance.CurrentDocument.DocumentSheet.NonFixedGlyphsExists)
                throw new InvalidContextException();

            Core.Instance.CurrentDocument.DocumentSheet.SelectAllGlyphs();

            Core.Instance.CurrentDocument.DocumentSheet.UpdateGlyphsCommands();

            Core.Instance.Redraw();
        }
    }
}

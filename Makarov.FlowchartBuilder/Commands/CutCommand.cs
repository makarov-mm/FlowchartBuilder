namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class CutCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            if (!Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphsExists)
                throw new InvalidContextException(@"Selected glyphs not exists.");

            Core.Instance.Buffer.Clear();
            foreach (var glyph in Core.Instance.CurrentDocument.DocumentSheet.SelectedGlyphs)
                Core.Instance.Buffer.Add(glyph);

            if (Core.Instance.Buffer.Count == 0)
                throw new InvalidContextException();

            Core.Instance.CurrentDocument.DocumentSheet.DeleteSelectedGlyphs();

            Enabled = false;

            GetInstance("PasteCommand").Enabled = true;

            Core.Instance.CurrentDocument.DocumentSheet.UpdateGlyphsCommands();

            Core.Instance.Redraw();
        }
    }
}

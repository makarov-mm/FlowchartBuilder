namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class FixAllCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            if (!Core.Instance.CurrentDocument.DocumentSheet.GlyphsExists)
                throw new InvalidContextException(@"Glyphs not exists.");

            foreach (var glyph in Core.Instance.CurrentDocument.DocumentSheet.Glyphs)
                glyph.Fixed = true;

            Core.Instance.CurrentDocument.DocumentSheet.UpdateGlyphsCommands();

            Core.Instance.Redraw();
        }
    }
}

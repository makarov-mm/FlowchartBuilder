namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class ZoomInCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            Core.Instance.CurrentDocument.DocumentSheet.ScaleFactor *= 1.1f;

            Core.Instance.Redraw();
        }
    }
}

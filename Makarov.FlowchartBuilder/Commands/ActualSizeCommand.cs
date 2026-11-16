namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class ActualSizeCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            Core.Instance.CurrentDocument.DocumentSheet.ScaleFactor = 1f;

            Core.Instance.Redraw();
        }
    }
}

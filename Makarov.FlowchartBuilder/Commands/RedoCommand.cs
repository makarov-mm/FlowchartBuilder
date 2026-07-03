namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class RedoCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            Core.Instance.CurrentDocument.HistoryForward();
            Core.Instance.Redraw();
        }
    }
}

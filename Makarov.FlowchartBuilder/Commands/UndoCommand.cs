namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class UndoCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            Core.Instance.CurrentDocument.HistoryBack();
            Core.Instance.Redraw();
        }
    }
}

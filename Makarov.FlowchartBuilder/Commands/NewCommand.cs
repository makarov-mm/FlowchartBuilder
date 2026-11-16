namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class NewCommand : Command
    {

        public override void Run()
        {

            Core.Instance.AddDocument(new Document());
            Core.Instance.Redraw();
        }
    }
}

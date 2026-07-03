namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class ExitCommand : Command
    {

        public override void Run()
        {
            Core.Instance.MainWindow.Close();
        }
    }
}

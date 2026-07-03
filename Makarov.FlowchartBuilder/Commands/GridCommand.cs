namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class GridCommand : Command
    {

        public override void Run()
        {

            Settings.Environment.Grid = !Settings.Environment.Grid;

            Checked = Settings.Environment.Grid;

            Core.Instance.Redraw();
        }
    }
}

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class SizeLocationLabelCommand : Command
    {

        public override void Run()
        {

            Settings.Environment.SizeLocationLabel = !Settings.Environment.SizeLocationLabel;

            Checked = Settings.Environment.SizeLocationLabel;

            Core.Instance.Redraw();
        }
    }
}

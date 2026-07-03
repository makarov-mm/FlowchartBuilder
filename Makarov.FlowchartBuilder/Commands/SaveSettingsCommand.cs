namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class SaveSettingsCommand : Command
    {

        public override void Run()
        {
            SettingsExport.ShowSaveDialog();
        }
    }
}

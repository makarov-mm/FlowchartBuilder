namespace Makarov.FlowchartBuilder.Commands;

public sealed class LoadSettingsCommand : Command
{
    public override void Run() => SettingsExport.ShowLoadDialog();
}

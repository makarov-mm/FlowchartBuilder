namespace Makarov.FlowchartBuilder.Commands;

public sealed class ResetWindowLocationsCommand : Command
{
    public override void Run() => Core.Instance.SetDefaultWindowsPos();
}

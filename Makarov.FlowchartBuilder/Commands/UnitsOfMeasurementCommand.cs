namespace Makarov.FlowchartBuilder.Commands;

public sealed class UnitsOfMeasurementCommand : CommandWithSubcommands
{
    public override IEnumerable<string> Commands =>
        Core.Instance.Units.LoadedUnits.OrderBy(units => Core.Instance.CurrentTranslator[units], StringComparer.CurrentCulture);

    public override void Run(string subcommand)
    {
        Settings.Environment.CurrentUnits = Core.Instance.Units[subcommand];
        Core.Instance.Redraw();
    }
}

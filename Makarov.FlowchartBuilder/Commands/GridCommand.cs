namespace Makarov.FlowchartBuilder.Commands;

public sealed class GridCommand : ToggleCommand
{
    public override bool IsOn
    {
        get => Settings.Environment.Grid;
        set => Settings.Environment.Grid = value;
    }
}

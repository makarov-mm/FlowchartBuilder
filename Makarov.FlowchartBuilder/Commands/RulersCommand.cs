namespace Makarov.FlowchartBuilder.Commands;

public sealed class RulersCommand : ToggleCommand
{
    public override bool IsOn
    {
        get => Settings.Environment.Rulers;
        set => Settings.Environment.Rulers = value;
    }
}

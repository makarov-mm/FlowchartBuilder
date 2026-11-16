namespace Makarov.FlowchartBuilder.Commands;

public sealed class SizeLocationLabelCommand : ToggleCommand
{
    public override bool IsOn
    {
        get => Settings.Environment.SizeLocationLabel;
        set => Settings.Environment.SizeLocationLabel = value;
    }
}

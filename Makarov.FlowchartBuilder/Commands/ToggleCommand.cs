namespace Makarov.FlowchartBuilder.Commands;

/// <summary>
/// An on/off view option whose menu item and toolbar button show its state with a check mark.
/// </summary>
public abstract class ToggleCommand : Command
{
    /// <summary>The persisted state of the option.</summary>
    public abstract bool IsOn { get; set; }

    public sealed override void Run()
    {
        IsOn = !IsOn;
        Checked = IsOn;
        OnToggled();
        Core.Instance.Redraw();
    }

    protected virtual void OnToggled()
    {
    }
}

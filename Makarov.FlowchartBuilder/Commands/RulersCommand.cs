using Makarov.FlowchartBuilder.Forms;

namespace Makarov.FlowchartBuilder.Commands;

public sealed class RulersCommand : ToggleCommand
{
    public override bool IsOn
    {
        get => Settings.Environment.Rulers;
        set => Settings.Environment.Rulers = value;
    }

    /// <summary>The rulers change the work area, so docked tool windows follow its new edges.</summary>
    protected override void OnToggled()
    {
        foreach (DockingForm window in Core.Instance.Windows.Windows.OfType<DockingForm>())
            window.UpdateDockedWindowPosition();
    }
}

using Makarov.FlowchartBuilder.Forms;

namespace Makarov.FlowchartBuilder.Commands;

public sealed class PaletteWindowCommand : Command
{
    /// <summary>Shows the tool window again after the user closed (hid) it.</summary>
    public override void Run() => Core.Instance.Windows.GetWindow<GlyphsForm>().ShowTool();
}

using Makarov.FlowchartBuilder.Forms;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class PaletteWindowCommand : Command
    {

        public override void Run()
        {

            Enabled = false;

            var glyphsFormWindow = Core.Instance.Windows.GetWindow<GlyphsForm>();
            glyphsFormWindow.Visible = !glyphsFormWindow.Visible;

            Core.Instance.Redraw();
        }
    }
}

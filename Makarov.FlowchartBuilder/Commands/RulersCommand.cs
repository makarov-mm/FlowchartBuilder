using Makarov.FlowchartBuilder.Forms;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class RulersCommand : Command
    {

        public override void Run()
        {
            using (new DonNotRedraw())
            {

                Settings.Environment.Rulers = !Settings.Environment.Rulers;

                foreach (var wnd in Core.Instance.Windows.Windows)
                    if (wnd is DockingForm)
                        ((DockingForm) wnd).UpdateDockedWindowPosition();
            }

            Checked = Settings.Environment.Rulers;

            Core.Instance.Redraw();
        }
    }
}

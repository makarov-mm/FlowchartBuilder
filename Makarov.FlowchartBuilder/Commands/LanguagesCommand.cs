using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using BaseSettings = Makarov.Framework.Core.Settings;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class LanguagesCommand : CommandWithSubcommands
    {

        public override void Run()
        { }

        public override void Run(string name)
        {
            Settings.Environment.Languages.CurrentLanguage = name;

            MessageBox.Show(
                Core.Instance.CurrentTranslator["Msg_Need_Restart_Application"],
                Core.Instance.CurrentTranslator["Msg_Info"],
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        public override IEnumerable<string> Commands
        {
            get
            {
                return Core.Instance.Translators.Languages.OrderBy(x => Core.Instance.CurrentTranslator[x]);
            }
        }

        public override Bitmap Icon(string cmd)
        {
            return Core.Instance.Icons[cmd];
        }
    }
}

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BaseSettings = Makarov.Framework.Core.Settings;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class UnitsOfMeasurementCommand : CommandWithSubcommands
    {

        public override void Run()
        { }

        public override void Run(string name)
        {

            Settings.Environment.CurrentUnits = Core.Instance.Units[name];

            Core.Instance.Redraw();
        }

        public override IEnumerable<string> Commands
        {
            get
            {
                return Core.Instance.Units.LoadedUnits.OrderBy(x => Core.Instance.CurrentTranslator[x]);
            }
        }

        public override Bitmap Icon(string cmd)
        {

            return null;
        }
    }
}

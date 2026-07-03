using System.Collections.Generic;
using System.Drawing;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class ZoomCommand : CommandWithSubcommands
    {

        public override void Run()
        { }

        public override void Run(string name)
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            float val = int.Parse(name.Substring(0, name.Length - 1)) / 100f;
            Core.Instance.CurrentDocument.DocumentSheet.ScaleFactor = val;

            Core.Instance.Redraw();
        }

        public override IEnumerable<string> Commands
        {
            get
            {
                var vals = new List<int>
                                {
                                    400,
                                    200,
                                    150,
                                    125,
                                    100,
                                    75,
                                    50,
                                    25
                                };

                foreach (int val in vals)
                    yield return val + "%";
            }
        }

        public override Bitmap Icon(string cmd)
        {
            return null;
        }
    }
}

using System.Collections.Generic;
using System.Drawing;

namespace Makarov.FlowchartBuilder.Commands
{

    public abstract class CommandWithSubcommands : Command
    {

        public abstract void Run(string name);

        public abstract IEnumerable<string> Commands { get; }

        public virtual Bitmap Icon(string cmd)
        {
            return null;
        }
    }
}

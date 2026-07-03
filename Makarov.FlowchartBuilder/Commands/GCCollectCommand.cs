using System;

namespace Makarov.FlowchartBuilder.Commands
{
    public sealed class GCCollectCommand : Command
    {
        public override void Run()
        {
            GC.Collect(0, GCCollectionMode.Forced);
        }
    }
}

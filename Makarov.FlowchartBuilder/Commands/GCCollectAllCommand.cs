using System;

namespace Makarov.FlowchartBuilder.Commands
{
    public sealed class GCCollectAllCommand : Command
    {
        public override void Run()
        {
            for (int i = 0; i < GC.MaxGeneration; i++)
                GC.Collect(i, GCCollectionMode.Forced);
        }
    }
}

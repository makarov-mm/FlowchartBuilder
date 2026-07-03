using System;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class CloseAllCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            throw new NotImplementedException();
        }
    }
}

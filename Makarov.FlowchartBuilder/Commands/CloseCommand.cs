using System;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class CloseCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            throw new NotImplementedException();
        }
    }
}

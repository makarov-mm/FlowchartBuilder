using System;

namespace Makarov.FlowchartBuilder.Commands
{
    public abstract class CommandedGlyphCommand : Command
    {
        public Type GlyphType { get; set; }
    }
}

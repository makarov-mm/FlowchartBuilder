namespace Makarov.FlowchartBuilder.Commands;

/// <summary>Opens an empty infinite canvas.</summary>
public sealed class NewCommand : Command
{
    public override void Run()
    {
        var core = Core.Instance;
        core.AddDocument(new Document { Name = core.CreateUntitledName() });
    }
}

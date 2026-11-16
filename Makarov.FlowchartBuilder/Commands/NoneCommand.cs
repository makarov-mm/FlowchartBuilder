namespace Makarov.FlowchartBuilder.Commands;

/// <summary>
/// For command glyphs that only act as palette labels: picking one just clears the palette selection.
/// </summary>
public sealed class NoneCommand : Command
{
    public override void Run()
    {
    }
}

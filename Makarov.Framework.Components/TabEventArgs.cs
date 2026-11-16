namespace Makarov.Framework.Components;

public sealed class TabEventArgs(Tab tab) : EventArgs
{
    public Tab Tab { get; } = tab;
}

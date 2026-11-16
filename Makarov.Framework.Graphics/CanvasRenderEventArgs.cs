namespace Makarov.Framework.Graphics;

public sealed class CanvasRenderEventArgs(Canvas canvas) : EventArgs
{
    public Canvas Canvas { get; } = canvas;
}

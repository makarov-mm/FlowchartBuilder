using System.Numerics;

namespace Makarov.Framework.Graphics;

/// <summary>Undoes a transform or clip pushed on a <see cref="Canvas"/> when disposed.</summary>
public readonly struct CanvasScope : IDisposable
{
    private readonly Canvas? _canvas;
    private readonly Matrix3x2 _transform;
    private readonly bool _clip;

    internal CanvasScope(Canvas canvas, Matrix3x2 transform, bool clip)
    {
        _canvas = canvas;
        _transform = transform;
        _clip = clip;
    }

    public void Dispose() => _canvas?.Restore(_transform, _clip);
}

using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives;

public sealed class RoundedRectangle : Primitive
{
    public RoundedRectangle(int x, int y, int width, int height, float cornerDiameter)
        : base(x, y, width, height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cornerDiameter);
        CornerDiameter = cornerDiameter;
    }

    public float CornerDiameter { get; }

    protected override GraphicsPath CreatePath(int left, int top, int width, int height)
    {
        float d = CornerDiameter;
        float r = d / 2f;

        var path = new GraphicsPath();
        path.StartFigure();
        path.AddLine(left + r, top, left + width - r, top);
        path.AddArc(left + width - d, top, d, d, 270, 90);
        path.AddLine(left + width, top + r, left + width, top + height - r);
        path.AddArc(left + width - d, top + height - d, d, d, 0, 90);
        path.AddLine(left + width - r, top + height, left + r, top + height);
        path.AddArc(left, top + height - d, d, d, 90, 90);
        path.AddLine(left, top + height - r, left, top + r);
        path.AddArc(left, top, d, d, 180, 90);
        path.CloseFigure();
        return path;
    }
}

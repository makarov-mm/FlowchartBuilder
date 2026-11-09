using System.Drawing.Drawing2D;

namespace Makarov.Framework.Graphics.Primitives;

/// <summary>Sector of the inscribed ellipse; angles are in degrees, clockwise from the X axis.</summary>
public sealed class Pie(int x, int y, int width, int height, int startAngle, int sweepAngle) : Primitive(x, y, width, height)
{
    public int StartAngle { get; } = startAngle;

    public int SweepAngle { get; } = sweepAngle;

    protected override GraphicsPath CreatePath(int left, int top, int width, int height)
    {
        var path = new GraphicsPath();
        path.StartFigure();
        path.AddPie(left, top, width, height, StartAngle, SweepAngle);
        path.CloseFigure();
        return path;
    }
}

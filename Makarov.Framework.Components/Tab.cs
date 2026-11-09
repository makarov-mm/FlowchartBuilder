using System.Drawing.Drawing2D;

namespace Makarov.Framework.Components;

/// <summary>
/// A caption tab drawn by <see cref="TabsPanel"/>; <see cref="Id"/> is an arbitrary key chosen by the owner.
/// </summary>
public sealed class Tab(int id, string caption)
{
    private const int CornerSize = 5;

    public int Id { get; } = id;

    public string Caption { get; } = caption;

    public int MeasureWidth(Font font) => TextRenderer.MeasureText(Caption, font).Width;

    public void Draw(Graphics graphics, Rectangle bounds, Font font, bool isActive)
    {
        using GraphicsPath path = CreateOutline(bounds);
        using var background = new LinearGradientBrush(
            new Point(bounds.Left, bounds.Top),
            new Point(bounds.Left, bounds.Bottom + 1),
            SystemColors.Control,
            isActive ? SystemColors.ControlDark : SystemColors.ControlLight);
        using var border = new Pen(SystemColors.ActiveBorder);

        graphics.FillPath(background, path);
        graphics.DrawPath(border, path);

        TextRenderer.DrawText(
            graphics, Caption, font, bounds, SystemColors.ControlText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static GraphicsPath CreateOutline(Rectangle bounds)
    {
        var path = new GraphicsPath();
        path.StartFigure();
        path.AddLines(
        [
            new Point(bounds.Left, bounds.Bottom),
            new Point(bounds.Right, bounds.Bottom),
            new Point(bounds.Right, bounds.Top),
            new Point(bounds.Left + CornerSize, bounds.Top),
            new Point(bounds.Left, bounds.Top + CornerSize),
        ]);
        path.CloseFigure();
        return path;
    }
}

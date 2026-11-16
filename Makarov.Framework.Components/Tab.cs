using System.Drawing.Drawing2D;

namespace Makarov.Framework.Components;

/// <summary>A tab of a <see cref="TabsPanel"/>: an id chosen by the owner, a caption and a close button.</summary>
public sealed class Tab(int id, string caption)
{
    private const int CornerSize = 5;
    private const int CloseButtonSize = 8;
    private const int CloseButtonMargin = 6;

    public int Id { get; } = id;

    public string Caption { get; set; } = caption;

    /// <summary>Width needed for the caption and the close button.</summary>
    public int MeasureWidth(Font font) => TextRenderer.MeasureText(Caption, font).Width + CloseButtonSize + CloseButtonMargin;

    public static Rectangle GetCloseButton(Rectangle bounds) =>
        new(bounds.Right - CloseButtonMargin - CloseButtonSize, bounds.Top + (bounds.Height - CloseButtonSize) / 2, CloseButtonSize, CloseButtonSize);

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

        Rectangle close = GetCloseButton(bounds);
        var text = Rectangle.FromLTRB(bounds.Left + CornerSize, bounds.Top, close.Left - 2, bounds.Bottom);
        TextRenderer.DrawText(
            graphics, Caption, font, text, SystemColors.ControlText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        using var cross = new Pen(SystemColors.ControlDarkDark, 1.5f);
        SmoothingMode smoothing = graphics.SmoothingMode;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.DrawLine(cross, close.Left, close.Top, close.Right, close.Bottom);
        graphics.DrawLine(cross, close.Left, close.Bottom, close.Right, close.Top);
        graphics.SmoothingMode = smoothing;
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

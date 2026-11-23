using System.Drawing;
using Makarov.FlowchartBuilder;
using Xunit;

namespace FlowchartBuilder.Tests;

public class RevealGeometryTests
{
    [Fact]
    public void HiddenPointMovesToExposedCanvasWithRulersAndOverlappingFloatingWindows()
    {
        // The canvas starts at a negative screen X (second monitor) and below the ruler.
        var viewport = new Rectangle(-360, 80, 600, 400);
        Rectangle[] windows = [new(-170, 70, 280, 300), new(-20, 250, 290, 240)];
        var target = Core.FindRevealTarget(viewport, new PointF(0, 300), windows);
        Assert.True(target.HasValue);
        Assert.True(viewport.Contains(target.Value));
        Assert.DoesNotContain(windows, frame => frame.Contains(target.Value));
        Assert.Null(Core.FindRevealTarget(viewport, target.Value, windows));
    }

    [Fact]
    public void OffscreenPointCentersOnCanvasAndFullyObscuredCanvasDoesNotPan()
    {
        var viewport = new Rectangle(20, 40, 400, 200);
        var outside = new PointF(5, 5);
        Assert.Equal(new Point(220, 140), Core.FindRevealTarget(viewport, outside, []));
        Assert.Null(Core.FindRevealTarget(viewport, outside, [viewport]));
    }
}

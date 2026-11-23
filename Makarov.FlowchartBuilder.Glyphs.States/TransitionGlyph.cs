using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.States;

[GlyphFamily("States"), GlyphName("Transition"), GlyphOrder(2)]
public sealed class TransitionGlyph : ConnectorGlyph
{
    public override bool AcceptsEndpoint(BlockGlyph block) => block is StateGlyph;

    public override bool AllowsSelfConnection => true;

    protected override bool IsPropertyVisible(string propertyName) =>
        propertyName != nameof(ArrowHead) && base.IsPropertyVisible(propertyName);

    public override void DrawConnection(Canvas canvas, ConnectorRoute route, Stroke stroke, float arrowLength) =>
        DrawRoute(canvas, route, stroke, arrowHead: true, arrowLength);
}

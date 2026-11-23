using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.Uml;

[GlyphFamily("UML")]
public abstract class UmlConnectorGlyph : ConnectorGlyph
{
    protected UmlConnectorGlyph() => Routing = ConnectorRouting.Straight;

    public override bool AcceptsEndpoint(BlockGlyph block) => block is ClassGlyph;

    // The arrow notation is part of the UML relationship.
    protected override bool IsPropertyVisible(string propertyName) =>
        propertyName != nameof(ArrowHead) && base.IsPropertyVisible(propertyName);
}

[GlyphName("UML inheritance"), GlyphOrder(2)]
public sealed class InheritanceGlyph : UmlConnectorGlyph
{
    public override void DrawConnection(Canvas canvas, ConnectorRoute route, Stroke stroke, float arrowLength)
    {
        float length = arrowLength * 1.3f;
        using var line = route.CreateGeometry(length);
        canvas.DrawGeometry(line, stroke);
        using var triangle = route.CreateArrowhead(length, length);
        canvas.FillGeometry(triangle, Settings.Colors.Sheet);
        canvas.DrawGeometry(triangle, stroke with { Dash = null });
    }
}

[GlyphName("UML association"), GlyphOrder(3)]
public sealed class AssociationGlyph : UmlConnectorGlyph
{
    public override void DrawConnection(Canvas canvas, ConnectorRoute route, Stroke stroke, float arrowLength)
    {
        using var line = route.CreateGeometry();
        canvas.DrawGeometry(line, stroke);
        PointF tip = route.End;
        PointF direction = route.EndDirection;
        var tail = new PointF(tip.X - direction.X * arrowLength, tip.Y - direction.Y * arrowLength);
        float halfWidth = arrowLength * 0.45f;
        var wing = new PointF(-direction.Y * halfWidth, direction.X * halfWidth);
        var headStroke = stroke with { Dash = null };
        canvas.DrawLine(tip, new PointF(tail.X + wing.X, tail.Y + wing.Y), headStroke);
        canvas.DrawLine(tip, new PointF(tail.X - wing.X, tail.Y - wing.Y), headStroke);
    }
}

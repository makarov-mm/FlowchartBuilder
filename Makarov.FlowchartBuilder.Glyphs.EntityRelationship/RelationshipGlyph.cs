using System.ComponentModel;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs.EntityRelationship;

[GlyphFamily("Entity relationship"), GlyphName("One-to-many relationship"), GlyphOrder(2)]
public sealed class RelationshipGlyph : ConnectorGlyph
{
    public RelationshipGlyph() => Routing = ConnectorRouting.Orthogonal;

    public override bool AcceptsEndpoint(BlockGlyph block) => block is TableGlyph;
    public override bool AllowsSelfConnection => true;

    [ActiveProperty, Category("Entity relationship"), DisplayName("Foreign key field")]
    [Description("Name of the field in the child table that references the parent primary key.")]
    public new string Caption { get => base.Caption; set => base.Caption = value; }

    [Browsable(false)]
    public string ChildField { get => Caption; set => Caption = value; }

    protected override bool IsPropertyVisible(string propertyName) =>
        propertyName != nameof(ArrowHead) && base.IsPropertyVisible(propertyName);

    public override void DrawConnection(Canvas canvas, ConnectorRoute route, Stroke stroke, float arrowLength)
    {
        using var line = route.CreateGeometry();
        canvas.DrawGeometry(line, stroke);
        var markers = stroke with { Dash = null };
        var points = route.Polyline;
        var start = route.Start;
        var next = points.FirstOrDefault(point => point != start, start);
        float dx = next.X - start.X, dy = next.Y - start.Y;
        float distance = MathF.Sqrt(dx * dx + dy * dy);
        if (distance < 0.01f) return;
        float halfWidth = arrowLength * 0.45f;
        var parentBar = new PointF(start.X + dx / distance * arrowLength * 0.55f, start.Y + dy / distance * arrowLength * 0.55f);
        var normal = new PointF(-dy / distance * halfWidth, dx / distance * halfWidth);
        canvas.DrawLine(new(parentBar.X - normal.X, parentBar.Y - normal.Y), new(parentBar.X + normal.X, parentBar.Y + normal.Y), markers);

        var end = route.End;
        var direction = route.EndDirection;
        var fork = new PointF(end.X - direction.X * arrowLength, end.Y - direction.Y * arrowLength);
        normal = new PointF(-direction.Y * halfWidth, direction.X * halfWidth);
        canvas.DrawLine(fork, new(end.X - normal.X, end.Y - normal.Y), markers);
        canvas.DrawLine(fork, new(end.X + normal.X, end.Y + normal.Y), markers);
    }
}

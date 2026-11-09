namespace Makarov.FlowchartBuilder.Glyphs;

/// <summary>How a connector runs between its ports.</summary>
public enum ConnectorRouting
{
    /// <summary>Smooth curve that leaves and enters the ports at right angles to the block sides.</summary>
    Curved,

    /// <summary>Straight line from port to port.</summary>
    Straight,

    /// <summary>Horizontal and vertical segments only.</summary>
    Orthogonal,
}

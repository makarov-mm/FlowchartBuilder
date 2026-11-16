namespace Makarov.FlowchartBuilder;

/// <summary>
/// Unit of measurement defined in Data\Units.xml.
/// </summary>
public sealed class Units
{
    public Units(string name, string shortName, float inMM, int bigStep, int smallStep)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(shortName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(inMM);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bigStep);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(smallStep);

        Name = name;
        ShortName = shortName;
        InMM = inMM;
        BigStep = bigStep;
        SmallStep = smallStep;
    }

    public string Name { get; }

    public string ShortName { get; }

    /// <summary>How many of these units make one millimeter.</summary>
    public float InMM { get; }

    /// <summary>How many millimeters make one of these units.</summary>
    public float MMInThis => 1f / InMM;

    /// <summary>Ruler step with a label, in these units.</summary>
    public int BigStep { get; }

    /// <summary>Ruler tick step, in these units.</summary>
    public int SmallStep { get; }

    public override string ToString() =>
        $"Units '{Name}' (short name: '{ShortName}', in mm: {InMM}, big step: {BigStep}, small step: {SmallStep})";
}

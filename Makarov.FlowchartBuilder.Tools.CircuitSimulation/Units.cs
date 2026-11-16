using System.Globalization;

namespace Makarov.FlowchartBuilder.Tools.CircuitSimulation;

/// <summary>
/// Values with SI prefixes: "10k", "4.7k", "100n", "12V", "50Hz", "2Meg". Prefixes are
/// case-sensitive where it matters (m is milli, M mega); a trailing unit is optional.
/// </summary>
internal static class Units
{
    private static readonly string[] UnitWords = ["V", "A", "W", "Hz", "F", "H", "Ohm", "R", "Ω", "Ω"];

    /// <summary>Parses a magnitude with an optional prefix and unit: "4.7kOhm" is 4700.</summary>
    public static bool TryParse(string? text, out double value)
    {
        value = 0;
        ReadOnlySpan<char> s = text.AsSpan().Trim();

        int end = 0;
        while (end < s.Length && (char.IsAsciiDigit(s[end]) || s[end] is '.' or '-' or '+'))
            end++;
        if (end == 0 || !double.TryParse(s[..end], NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
            return false;

        ReadOnlySpan<char> rest = s[end..].TrimStart();
        double multiplier = 1;
        if (rest.StartsWith("Meg", StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1e6;
            rest = rest[3..];
        }
        else if (rest.Length > 0 && !IsUnit(rest))
        {
            multiplier = Multiplier(rest[0]);
            if (double.IsNaN(multiplier))
                return false;
            rest = rest[1..];
        }

        if (rest.Length > 0 && !IsUnit(rest))
            return false;

        value = number * multiplier;
        return double.IsFinite(value);
    }

    /// <summary>
    /// Parses a lamp: a rating such as "12V 5W", or a resistance with an optional rated power such
    /// as "28.8 5W". Gives the lamp's resistance and rated power.
    /// </summary>
    public static bool TryParseLampRating(string? text, out double resistance, out double ratedPower)
    {
        resistance = 100;
        ratedPower = 1;

        double? volts = null, watts = null, ohms = null;
        foreach (string part in Words(text))
        {
            if (!TryParse(part, out double v))
                continue;
            if (part.EndsWith('W') || part.EndsWith('w'))
                watts = v;
            else if (part.EndsWith('V') || part.EndsWith('v'))
                volts = v;
            else
                ohms = v;
        }

        if (volts is > 0 && watts is > 0)
        {
            resistance = volts.Value * volts.Value / watts.Value;
            ratedPower = watts.Value;
            return true;
        }
        if (ohms is > 0)
        {
            resistance = ohms.Value;
            ratedPower = watts is > 0 ? watts.Value : 1;
            return true;
        }
        return false;
    }

    /// <summary>Parses an AC source such as "5V 50Hz": amplitude and frequency, either of which may be left out.</summary>
    public static bool TryParseAcSpec(string? text, out double amplitude, out double frequency)
    {
        amplitude = 5;
        frequency = 50;

        bool any = false;
        foreach (string part in Words(text))
        {
            if (!TryParse(part, out double v))
                continue;
            if (part.EndsWith("Hz", StringComparison.OrdinalIgnoreCase))
                frequency = v;
            else
                amplitude = v;
            any = true;
        }
        return any;
    }

    /// <summary>Engineering notation for display: 0.0032 with unit "A" is "3.2 mA", without a unit "3.2m".</summary>
    public static string Format(double value, string unit = "")
    {
        if (Math.Abs(value) < 1e-12)
            return unit.Length == 0 ? "0" : "0 " + unit;

        (double scale, string prefix) = Scale(Math.Abs(value), ascii: false);
        string number = (value * scale).ToString("0.###", CultureInfo.InvariantCulture);
        return unit.Length == 0 ? number + prefix : $"{number} {prefix}{unit}";
    }

    /// <summary>A component value as the parser reads it back: 4700 is "4.7k", 1e-7 "100n", 2e6 "2Meg".</summary>
    public static string FormatValue(double value)
    {
        (double scale, string prefix) = Scale(Math.Abs(value), ascii: true);
        return (value * scale).ToString("0.###", CultureInfo.InvariantCulture) + prefix;
    }

    private static (double Scale, string Prefix) Scale(double magnitude, bool ascii) => magnitude switch
    {
        >= 1e9 => (1e-9, "G"),
        >= 1e6 => (1e-6, ascii ? "Meg" : "M"),
        >= 1e3 => (1e-3, "k"),
        >= 1 => (1, ""),
        >= 1e-3 => (1e3, "m"),
        >= 1e-6 => (1e6, ascii ? "u" : "µ"),
        >= 1e-9 => (1e9, "n"),
        _ => (1e12, "p"),
    };

    private static double Multiplier(char prefix) => prefix switch
    {
        'p' => 1e-12,
        'n' => 1e-9,
        'u' or 'µ' or 'μ' => 1e-6,
        'm' => 1e-3,
        'k' or 'K' => 1e3,
        'M' => 1e6,
        'G' => 1e9,
        _ => double.NaN,
    };

    private static bool IsUnit(ReadOnlySpan<char> s)
    {
        foreach (string unit in UnitWords)
        {
            if (s.Equals(unit, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static string[] Words(string? text) => (text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
}

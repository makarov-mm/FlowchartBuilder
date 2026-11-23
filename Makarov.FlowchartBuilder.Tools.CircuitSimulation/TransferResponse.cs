using System.Numerics;

namespace Makarov.FlowchartBuilder.Tools.CircuitSimulation;

/// <summary>Gain and phase of Vout/Vin at one frequency, both voltages relative to ground.</summary>
internal readonly record struct TransferResponse(double? GainDb, double? PhaseDegrees)
{
    public static TransferResponse Measure(Complex[] solution, int inputNode, int outputNode)
    {
        var input = CircuitSimulator.GetAcNodeVoltage(solution, inputNode);
        var output = CircuitSimulator.GetAcNodeVoltage(solution, outputNode);
        if (input == Complex.Zero) return new(null, null);
        if (output == Complex.Zero) return new(double.NegativeInfinity, null);

        // Subtract logarithms before scaling to avoid overflowing a large voltage ratio.
        double gain = 20 * (Math.Log10(output.Magnitude) - Math.Log10(input.Magnitude));
        double phase = Math.IEEERemainder(output.Phase - input.Phase, 2 * Math.PI) * 180 / Math.PI;
        return new(gain, phase);
    }
}

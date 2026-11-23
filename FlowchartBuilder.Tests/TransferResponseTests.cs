using System.Numerics;
using Makarov.FlowchartBuilder.Tools.CircuitSimulation;
using Xunit;

namespace FlowchartBuilder.Tests;

public sealed class TransferResponseTests
{
    [Theory]
    [InlineData("1uF", 159.15)]
    [InlineData("2uF", 79.58)]
    public void CursorMeasuresRcCutoffForBothCapacitances(string capacitance, double frequency)
    {
        var model = LowPass(capacitance, "5V 50Hz");
        var reading = TransferResponse.Measure(model.SolveAc(frequency), 1, 2);
        Assert.InRange(reading.GainDb!.Value, -3.012, -3.009);
        Assert.InRange(reading.PhaseDegrees!.Value, -45.01, -44.99);
    }

    [Fact]
    public void OffGridFrequencyUsesTheComplexRatioAndLeavesTransientStateAlone()
    {
        const double frequency = 137.24;
        var model = LowPass("1uF", "12V 50Hz");
        model.Step(0.001);
        double time = model.Time;
        double voltage = model.GetNodeVoltage(2);
        var solution = model.SolveAc(frequency);
        var forward = TransferResponse.Measure(solution, 1, 2);
        var reverse = TransferResponse.Measure(solution, 2, 1);
        double omegaRc = 2 * Math.PI * frequency * 0.001;
        Assert.Equal(-10 * Math.Log10(1 + omegaRc * omegaRc), forward.GainDb!.Value, 4);
        Assert.Equal(-Math.Atan(omegaRc) * 180 / Math.PI, forward.PhaseDegrees!.Value, 4);
        Assert.Equal(-forward.GainDb.Value, reverse.GainDb!.Value, 10);
        Assert.Equal(-forward.PhaseDegrees.Value, reverse.PhaseDegrees!.Value, 10);
        Assert.Equal(time, model.Time);
        Assert.Equal(voltage, model.GetNodeVoltage(2));
    }

    [Fact]
    public void GroundAndUndrivenNodesHaveExplicitUndefinedMeasurements()
    {
        Complex[] nodes = [new(5, 0), Complex.Zero];
        foreach (int zeroInput in new[] { 0, 2 })
        {
            var reading = TransferResponse.Measure(nodes, zeroInput, 1);
            Assert.Null(reading.GainDb);
            Assert.Null(reading.PhaseDegrees);
        }
        var zeroOutput = TransferResponse.Measure(nodes, 1, 0);
        Assert.Equal(double.NegativeInfinity, zeroOutput.GainDb!.Value);
        Assert.Null(zeroOutput.PhaseDegrees);
        var sameNode = TransferResponse.Measure(nodes, 1, 1);
        Assert.Equal(0.0, sameNode.GainDb!.Value);
        Assert.Equal(0.0, sameNode.PhaseDegrees!.Value);
    }

    [Fact]
    public void PhaseWrapsAtAFullTurnAndWideVoltageRatiosStayFinite()
    {
        Complex[] nodes = [Complex.FromPolarCoordinates(1, 170 * Math.PI / 180),
            Complex.FromPolarCoordinates(1, -170 * Math.PI / 180)];
        Assert.Equal(20, TransferResponse.Measure(nodes, 1, 2).PhaseDegrees!.Value, 10);
        Complex[] wideRange = [new(1e-200, 0), new(1e200, 0)];
        Assert.Equal(8000, TransferResponse.Measure(wideRange, 1, 2).GainDb!.Value, 6);
    }

    private static CircuitSimulator LowPass(string capacitance, string source) =>
        CircuitSimulator.Build(new(["GND", "in", "out"],
        [
            new("V1", SymbolKind.AcSource, source, 1, 0),
            new("R1", SymbolKind.Resistor, "1k", 1, 2),
            new("C1", SymbolKind.Capacitor, capacitance, 2, 0)
        ]));
}

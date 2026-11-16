using System.Numerics;

namespace Makarov.FlowchartBuilder.Tools.CircuitSimulation;

/// <summary>
/// Circuit simulation by Modified Nodal Analysis: in the time domain (<see cref="Step"/>) and as a
/// small-signal frequency analysis (<see cref="SolveAc"/>).
///
/// Capacitors and inductors use backward-Euler companion models (L-stable, so even an ideal source
/// across a closed switch stays calm). Diodes are piecewise linear, off or on with Von and Ron, and
/// resolved by state iteration within each step. Switches read the live
/// <see cref="CircuitPart.StateOn"/>, so they can be toggled while the simulation runs. A fuse
/// blows the moment its rated current is exceeded and stays blown until <see cref="Reset"/>.
///
/// The dense system is assembled and solved from scratch every step: at schematic scale (tens of
/// nodes) that takes microseconds and keeps the code obvious.
/// </summary>
internal sealed class CircuitSimulator
{
    private const double Gmin = 1e-9;   // from every node to ground; also an open switch, a blown fuse, a blocking diode
    private const double SwitchOnResistance = 1e-3;
    private const double FuseResistance = 1e-2;
    private const double DiodeVon = 0.7;
    private const double DiodeRon = 0.05;
    private const int MaxDiodeIterations = 12;

    private sealed class Element(CircuitPart symbol, int nodeA, int nodeB)
    {
        public CircuitPart Symbol { get; } = symbol;
        public SymbolKind Kind { get; } = symbol.Kind;
        public int NodeA { get; } = nodeA;   // pin 1: + terminal, anode
        public int NodeB { get; } = nodeB;   // pin 2: - terminal, cathode

        public double Value;                 // Ω, F, H or V; the amplitude of an AC source
        public double Frequency;             // AC sources, Hz
        public double RatedPower;            // lamps, W
        public double RatedCurrent;          // fuses, A
        public int Branch = -1;              // the row of the element's current (sources, inductors)

        // State carried from step to step.
        public double PreviousVoltage;       // capacitors
        public double PreviousCurrent;       // inductors
        public bool DiodeOn;
        public bool FuseBlown;
        public double Current;               // from A to B, as last solved
    }

    public CircuitNetwork Network { get; }
    private readonly List<Element> _elements;
    private readonly Dictionary<CircuitPart, Element> _elementOf;
    private readonly int _nodeCount;         // nodes besides ground; node n is unknown n - 1
    private readonly double[,] _matrix;
    private readonly double[] _rhs;
    private readonly double[] _solution;     // node voltages, then branch currents

    private CircuitSimulator(CircuitNetwork network, List<Element> elements, List<string> warnings)
    {
        Network = network;
        _nodeCount = network.Nodes.Count - 1;
        _elements = elements;
        _elementOf = elements.ToDictionary(e => e.Symbol);
        Warnings = warnings;

        int size = _nodeCount;
        foreach (Element e in elements)
        {
            if (e.Kind is SymbolKind.DcSource or SymbolKind.AcSource or SymbolKind.Inductor)
                e.Branch = size++;
        }

        _matrix = new double[size, size];
        _rhs = new double[size];
        _solution = new double[size];
    }

    public double Time { get; private set; }

    /// <summary>What was simulated differently than drawn: unparsable values, skipped symbols.</summary>
    public IReadOnlyList<string> Warnings { get; }

    /// <summary>Whether there is an AC source to drive a frequency analysis.</summary>
    public bool HasAcSource => _elements.Exists(e => e.Kind == SymbolKind.AcSource);

    public static CircuitSimulator Build(CircuitNetwork network)
    {
        List<string> warnings = [];
        var elements = network.Parts.Select(p => CreateElement(p, warnings)).ToList();
        return new CircuitSimulator(network, elements, warnings);
    }

    private static Element CreateElement(CircuitPart symbol, List<string> warnings)
    {
        var e = new Element(symbol, symbol.NodeA, symbol.NodeB);
        switch (e.Kind)
        {
            case SymbolKind.Resistor:
                e.Value = PositiveValue(symbol, 1e3, "1k", warnings);
                break;

            case SymbolKind.Capacitor:
                e.Value = PositiveValue(symbol, 100e-9, "100n", warnings);
                break;

            case SymbolKind.Inductor:
                e.Value = PositiveValue(symbol, 10e-3, "10m", warnings);
                break;

            case SymbolKind.Lamp:
                if (!Units.TryParseLampRating(symbol.Value, out e.Value, out e.RatedPower))
                {
                    warnings.Add(CannotParse(symbol, "12V 5W"));
                    (e.Value, e.RatedPower) = (12 * 12 / 5.0, 5);
                }
                break;

            case SymbolKind.Fuse:
                e.Value = FuseResistance;
                e.RatedCurrent = PositiveValue(symbol, 1, "1A", warnings);
                break;

            case SymbolKind.DcSource:
                if (!Units.TryParse(symbol.Value, out e.Value))
                {
                    warnings.Add(CannotParse(symbol, "5V"));
                    e.Value = 5;
                }
                break;

            case SymbolKind.AcSource:
                if (!Units.TryParseAcSpec(symbol.Value, out e.Value, out e.Frequency) || e.Frequency <= 0)
                {
                    warnings.Add(CannotParse(symbol, "5V 50Hz"));
                    (e.Value, e.Frequency) = (5, 50);
                }
                break;

            case SymbolKind.Switch or SymbolKind.Diode:
                break;

            default:
                throw new InvalidOperationException($"Unsupported component: {symbol.Kind}.");
        }
        return e;
    }

    private static double PositiveValue(CircuitPart symbol, double fallback, string fallbackText, List<string> warnings)
    {
        if (Units.TryParse(symbol.Value, out double value) && value > 0)
            return value;
        warnings.Add(CannotParse(symbol, fallbackText));
        return fallback;
    }

    private static string CannotParse(CircuitPart symbol, string fallback) =>
        $"{symbol.DisplayName}: cannot parse '{symbol.Value}', using {fallback}.";

    // ------------------------------------------------------------------ transient analysis

    /// <summary>Back to time zero: capacitors and inductors discharged, diodes off, fuses whole.</summary>
    public void Reset()
    {
        Time = 0;
        foreach (Element e in _elements)
        {
            e.PreviousVoltage = 0;
            e.PreviousCurrent = 0;
            e.DiodeOn = false;
            e.FuseBlown = false;
            e.Current = 0;
        }
        Array.Clear(_solution);
    }

    /// <summary>Advances the simulation by one time step. Throws InvalidOperationException for a singular circuit.</summary>
    public void Step(double dt)
    {
        if (!double.IsFinite(dt) || dt <= 0) throw new ArgumentOutOfRangeException(nameof(dt));
        Time += dt;
        SolveAndUpdate(dt);
    }

    /// <summary>DC equilibrium: C/dt and L/dt vanish, capacitors open and inductors short.</summary>
    public void SolveOperatingPoint()
    {
        Reset();
        int blown;
        do
        {
            blown = _elements.Count(e => e.FuseBlown);
            SolveAndUpdate(double.PositiveInfinity);
        } while (blown != _elements.Count(e => e.FuseBlown));
    }

    private void SolveAndUpdate(double dt)
    {
        // Diodes: assume their states, solve, correct the states that the solution contradicts, repeat.
        for (int iteration = 0; ; iteration++)
        {
            Assemble(dt);
            DenseSolver.Solve(_matrix, _rhs, _solution, Math.Abs);
            if (CorrectDiodeStates()) break;
            if (iteration >= MaxDiodeIterations) throw new InvalidOperationException("Diode iteration did not converge. Reduce the time step.");
        }

        foreach (Element e in _elements)
            UpdateState(e, dt);
    }

    private void Assemble(double dt)
    {
        Array.Clear(_matrix);
        Array.Clear(_rhs);

        for (int i = 0; i < _nodeCount; i++)
            _matrix[i, i] += Gmin;

        foreach (Element e in _elements)
        {
            switch (e.Kind)
            {
                case SymbolKind.Resistor or SymbolKind.Lamp:
                    StampConductance(_matrix, e, 1.0 / e.Value);
                    break;

                case SymbolKind.Fuse:
                    StampConductance(_matrix, e, e.FuseBlown ? Gmin : 1.0 / e.Value);
                    break;

                case SymbolKind.Switch:
                    StampConductance(_matrix, e, e.Symbol.StateOn ? 1.0 / SwitchOnResistance : Gmin);
                    break;

                case SymbolKind.Capacitor:
                    double g = e.Value / dt;
                    StampConductance(_matrix, e, g);
                    StampCurrent(e, g * e.PreviousVoltage);
                    break;

                case SymbolKind.Diode:
                    if (e.DiodeOn)
                    {
                        StampConductance(_matrix, e, 1.0 / DiodeRon);
                        StampCurrent(e, DiodeVon / DiodeRon);
                    }
                    else
                    {
                        StampConductance(_matrix, e, Gmin);
                    }
                    break;

                case SymbolKind.Inductor:
                    StampBranch(_matrix, e);
                    _matrix[e.Branch, e.Branch] -= e.Value / dt;
                    _rhs[e.Branch] -= e.Value / dt * e.PreviousCurrent;
                    break;

                case SymbolKind.DcSource:
                    StampBranch(_matrix, e);
                    _rhs[e.Branch] = e.Value;
                    break;

                case SymbolKind.AcSource:
                    StampBranch(_matrix, e);
                    _rhs[e.Branch] = e.Value * Math.Sin(2.0 * Math.PI * e.Frequency * Time);
                    break;
            }
        }
    }

    // A conducting diode turns off when its current would be negative, a blocking one turns on
    // above Von. True when no diode had to change.
    private bool CorrectDiodeStates()
    {
        bool consistent = true;
        foreach (Element e in _elements)
        {
            if (e.Kind != SymbolKind.Diode)
                continue;
            double v = Voltage(e);
            if (e.DiodeOn ? v < DiodeVon : v > DiodeVon)
            {
                e.DiodeOn = !e.DiodeOn;
                consistent = false;
            }
        }
        return consistent;
    }

    private void UpdateState(Element e, double dt)
    {
        double v = Voltage(e);
        switch (e.Kind)
        {
            case SymbolKind.Resistor or SymbolKind.Lamp:
                e.Current = v / e.Value;
                break;

            case SymbolKind.Fuse:
                e.Current = e.FuseBlown ? v * Gmin : v / e.Value;
                if (!e.FuseBlown && Math.Abs(e.Current) > e.RatedCurrent)
                {
                    e.FuseBlown = true;
                    e.Current = 0;
                }
                break;

            case SymbolKind.Switch:
                e.Current = e.Symbol.StateOn ? v / SwitchOnResistance : v * Gmin;
                break;

            case SymbolKind.Capacitor:
                e.Current = e.Value / dt * (v - e.PreviousVoltage);
                e.PreviousVoltage = v;
                break;

            case SymbolKind.Inductor:
                e.Current = e.PreviousCurrent = _solution[e.Branch];
                break;

            case SymbolKind.Diode:
                e.Current = e.DiodeOn ? (v - DiodeVon) / DiodeRon : v * Gmin;
                break;

            case SymbolKind.DcSource or SymbolKind.AcSource:
                e.Current = -_solution[e.Branch];   // delivered: out of the + terminal into the circuit
                break;
        }
    }

    private double Voltage(Element e) => GetNodeVoltage(e.NodeA) - GetNodeVoltage(e.NodeB);

    /// <summary>A current source driving <paramref name="current"/> into node A and out of node B.</summary>
    private void StampCurrent(Element e, double current)
    {
        if (e.NodeA != 0)
            _rhs[e.NodeA - 1] += current;
        if (e.NodeB != 0)
            _rhs[e.NodeB - 1] -= current;
    }

    private static void StampConductance<T>(T[,] a, Element e, T g) where T : INumberBase<T>
    {
        int ra = e.NodeA - 1, rb = e.NodeB - 1;   // ground, node 0, has no row
        if (ra >= 0)
            a[ra, ra] += g;
        if (rb >= 0)
            a[rb, rb] += g;
        if (ra >= 0 && rb >= 0)
        {
            a[ra, rb] -= g;
            a[rb, ra] -= g;
        }
    }

    /// <summary>The KCL entries of a branch current and its KVL row, shared by sources and inductors.</summary>
    private static void StampBranch<T>(T[,] a, Element e) where T : INumberBase<T>
    {
        int k = e.Branch;
        if (e.NodeA != 0)
        {
            a[e.NodeA - 1, k] += T.One;
            a[k, e.NodeA - 1] += T.One;
        }
        if (e.NodeB != 0)
        {
            a[e.NodeB - 1, k] -= T.One;
            a[k, e.NodeB - 1] -= T.One;
        }
    }

    // ------------------------------------------------------------------ results

    public double GetNodeVoltage(int node) => node == 0 ? 0 : _solution[node - 1];

    /// <summary>
    /// The current through a two-terminal symbol from pin 1 to pin 2; for a source the current it
    /// delivers. Null for symbols that are not simulated.
    /// </summary>
    public double? GetCurrent(CircuitPart symbol) => _elementOf.TryGetValue(symbol, out Element? e) ? e.Current : null;

    /// <summary>A lamp's brightness, 0 to 1 at its rated power.</summary>
    public double GetLampBrightness(CircuitPart lamp)
    {
        if (!_elementOf.TryGetValue(lamp, out Element? e) || e.Kind != SymbolKind.Lamp)
            return 0;
        double v = Voltage(e);
        return Math.Clamp(v * v / e.Value / e.RatedPower, 0, 1);
    }

    public bool IsFuseBlown(CircuitPart fuse) => _elementOf.TryGetValue(fuse, out Element? e) && e.FuseBlown;

    // ------------------------------------------------------------------ AC analysis

    /// <summary>
    /// Small-signal analysis at one frequency, as SPICE's AC analysis: the same topology with complex
    /// admittances, jωC for capacitors and jωL in the inductor branches. Nonlinear elements keep their
    /// present state: a diode its segment, a switch its position, a fuse whether it has blown. AC
    /// sources drive the circuit with their amplitude; DC sources become shorts, as superposition
    /// demands. Gives the node voltages followed by the branch currents. Throws
    /// InvalidOperationException for a singular circuit.
    /// </summary>
    public Complex[] SolveAc(double frequency)
    {
        int size = _rhs.Length;
        var a = new Complex[size, size];
        var rhs = new Complex[size];
        var x = new Complex[size];
        double w = 2.0 * Math.PI * frequency;

        for (int i = 0; i < _nodeCount; i++)
            a[i, i] += Gmin;

        foreach (Element e in _elements)
        {
            switch (e.Kind)
            {
                case SymbolKind.Resistor or SymbolKind.Lamp:
                    StampConductance(a, e, 1.0 / e.Value);
                    break;

                case SymbolKind.Fuse:
                    StampConductance(a, e, e.FuseBlown ? Gmin : 1.0 / e.Value);
                    break;

                case SymbolKind.Switch:
                    StampConductance(a, e, e.Symbol.StateOn ? 1.0 / SwitchOnResistance : Gmin);
                    break;

                case SymbolKind.Capacitor:
                    StampConductance(a, e, new Complex(0, w * e.Value));
                    break;

                case SymbolKind.Diode:
                    StampConductance(a, e, e.DiodeOn ? 1.0 / DiodeRon : Gmin);
                    break;

                case SymbolKind.Inductor:
                    StampBranch(a, e);
                    a[e.Branch, e.Branch] -= new Complex(0, w * e.Value);
                    break;

                case SymbolKind.DcSource:
                    StampBranch(a, e);
                    break;

                case SymbolKind.AcSource:
                    StampBranch(a, e);
                    rhs[e.Branch] = e.Value;
                    break;
            }
        }

        DenseSolver.Solve(a, rhs, x, static c => c.Magnitude);
        return x;
    }

    /// <summary>A node voltage from a <see cref="SolveAc"/> solution; node 0 is ground.</summary>
    public static Complex GetAcNodeVoltage(Complex[] solution, int node) => node == 0 ? Complex.Zero : solution[node - 1];

    /// <summary>
    /// The frequency response of nodes: response[k][i] is the voltage of nodes[k] at frequencies[i].
    /// Throws InvalidOperationException for a singular circuit.
    /// </summary>
    public Complex[][] SweepAc(IReadOnlyList<int> nodes, IReadOnlyList<double> frequencies)
    {
        Complex[][] response = [.. nodes.Select(_ => new Complex[frequencies.Count])];
        for (int i = 0; i < frequencies.Count; i++)
        {
            Complex[] solution = SolveAc(frequencies[i]);
            for (int k = 0; k < nodes.Count; k++)
                response[k][i] = GetAcNodeVoltage(solution, nodes[k]);
        }
        return response;
    }

    /// <summary>Log-spaced frequencies from <paramref name="from"/> over whole decades, up to at least <paramref name="to"/>.</summary>
    public static double[] LogFrequencies(double from, double to, int pointsPerDecade)
    {
        int count = (int)Math.Ceiling(Math.Log10(to / from)) * pointsPerDecade + 1;
        return [.. Enumerable.Range(0, count).Select(i => from * Math.Pow(10, (double)i / pointsPerDecade))];
    }
}

/// <summary>Dense Gaussian elimination with partial pivoting, for real and complex systems.</summary>
file static class DenseSolver
{
    /// <summary>Solves a·x = b, overwriting a and b. Throws InvalidOperationException when a is singular.</summary>
    public static void Solve<T>(T[,] a, T[] b, T[] x, Func<T, double> magnitude) where T : INumberBase<T>
    {
        int n = b.Length;
        for (int col = 0; col < n; col++)
        {
            int pivot = col;
            double best = magnitude(a[col, col]);
            for (int r = col + 1; r < n; r++)
            {
                double m = magnitude(a[r, col]);
                if (m > best)
                {
                    best = m;
                    pivot = r;
                }
            }
            if (best < 1e-14)
                throw new InvalidOperationException("Singular matrix: the circuit has no unique solution.");

            if (pivot != col)
            {
                for (int c = col; c < n; c++)
                    (a[col, c], a[pivot, c]) = (a[pivot, c], a[col, c]);
                (b[col], b[pivot]) = (b[pivot], b[col]);
            }

            for (int r = col + 1; r < n; r++)
            {
                T f = a[r, col] / a[col, col];
                if (f == T.Zero)
                    continue;
                for (int c = col; c < n; c++)
                    a[r, c] -= f * a[col, c];
                b[r] -= f * b[col];
            }
        }

        for (int r = n - 1; r >= 0; r--)
        {
            T sum = b[r];
            for (int c = r + 1; c < n; c++)
                sum -= a[r, c] * x[c];
            x[r] = sum / a[r, r];
        }
    }
}

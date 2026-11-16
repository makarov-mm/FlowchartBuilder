using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Tools.CircuitSimulation;

internal enum SymbolKind { Resistor, Capacitor, Inductor, DcSource, AcSource, Switch, Fuse, Lamp, Diode }

internal sealed class CircuitPart(string displayName, SymbolKind kind, string value, int nodeA, int nodeB)
{
    public string DisplayName { get; } = displayName;
    public SymbolKind Kind { get; } = kind;
    public string Value { get; } = value;
    public int NodeA { get; } = nodeA;
    public int NodeB { get; } = nodeB;
    public bool StateOn { get; set; }
}

internal sealed record CircuitNetwork(IReadOnlyList<string> Nodes, IReadOnlyList<CircuitPart> Parts)
{
    public static CircuitNetwork From(Document document)
    {
        var glyphs = document.DocumentSheet.Glyphs;
        var blocks = glyphs.OfType<BlockGlyph>().Where(b => b.Family == "Electrical").Reverse().ToArray();
        if (blocks.Length == 0) throw new InvalidDataException("Place electrical components and connect their terminals with wires.");
        var terminals = new Dictionary<(BlockGlyph Block, int Pin), int>();
        var connected = new HashSet<(BlockGlyph, int)>();
        foreach (var b in blocks)
        {
            int count = b.Name switch { "Ground" => 1, "Wire junction" => 4, "NPN transistor" => 3, _ => 2 };
            for (int i = 0; i < count; i++) terminals[(b, i)] = terminals.Count;
        }
        var parent = Enumerable.Range(0, terminals.Count).ToArray();
        int Root(int i)
        {
            while (parent[i] != i) { parent[i] = parent[parent[i]]; i = parent[i]; }
            return i;
        }
        void Join(int a, int b) => parent[Root(a)] = Root(b);
        foreach (var junction in blocks.Where(b => b.Name == "Wire junction"))
            for (int i = 1; i < 4; i++) Join(terminals[(junction, 0)], terminals[(junction, i)]);
        foreach (var wire in glyphs.OfType<ConnectorGlyph>())
        {
            if (wire.Source?.Family != "Electrical" && wire.Target?.Family != "Electrical") continue;
            if (wire.Source == null || wire.Target == null ||
                !terminals.TryGetValue((wire.Source, wire.SourcePort), out int a) ||
                !terminals.TryGetValue((wire.Target, wire.TargetPort), out int b))
                throw new InvalidDataException("A circuit wire must connect two electrical terminals.");
            Join(a, b);
            connected.Add((wire.Source, wire.SourcePort));
            connected.Add((wire.Target, wire.TargetPort));
        }
        var grounds = blocks.Where(b => b.Name == "Ground" && connected.Contains((b, 0))).ToArray();
        if (grounds.Length == 0) throw new InvalidDataException("Connect a Ground symbol to define the 0 V reference.");
        int ground = terminals[(grounds[0], 0)];
        foreach (var g in grounds) Join(ground, terminals[(g, 0)]);
        var nodes = new Dictionary<int, int> { [Root(ground)] = 0 };
        var labels = new List<string> { "GND" };
        int Node(BlockGlyph block, int pin)
        {
            if (!connected.Contains((block, pin)))
                throw new InvalidDataException($"{block.Caption}: terminal {pin + 1} is unconnected.");
            int root = Root(terminals[(block, pin)]);
            if (!nodes.TryGetValue(root, out int index))
            {
                index = nodes.Count;
                nodes.Add(root, index);
                labels.Add($"N{index:000} ({block.Caption}.{pin + 1})");
            }
            return index;
        }
        var parts = new List<CircuitPart>();
        foreach (var block in blocks.Where(b => b.Name is not ("Ground" or "Wire junction")))
        {
            var kind = block.Name switch
            {
                "Battery" or "DC voltage source" => SymbolKind.DcSource,
                "AC voltage source" => SymbolKind.AcSource,
                "Resistor" => SymbolKind.Resistor, "Capacitor" => SymbolKind.Capacitor,
                "Inductor" => SymbolKind.Inductor, "Lamp" => SymbolKind.Lamp, "Fuse" => SymbolKind.Fuse,
                "Switch" => SymbolKind.Switch, "Diode" => SymbolKind.Diode,
                _ => throw new InvalidDataException($"{block.Caption}: no simulation model for {block.Name}.")
            };
            string value = block.Metadata.FindActiveProperty("Value")?.GetValue(block) as string ?? "";
            parts.Add(new CircuitPart(block.Caption, kind, value, Node(block, 0), Node(block, 1))
            {
                StateOn = block.Metadata.FindActiveProperty("Closed")?.GetValue(block) is true
            });
        }
        if (!parts.Any(p => p.Kind is SymbolKind.DcSource or SymbolKind.AcSource))
            throw new InvalidDataException("Add a battery or voltage source.");
        // Reject isolated subnetworks instead of reporting voltages determined by numerical leakage.
        var reached = new HashSet<int> { 0 };
        int before;
        do
        {
            before = reached.Count;
            foreach (var p in parts)
                if (reached.Contains(p.NodeA) || reached.Contains(p.NodeB))
                { reached.Add(p.NodeA); reached.Add(p.NodeB); }
        } while (before != reached.Count);
        if (reached.Count != labels.Count) throw new InvalidDataException("A subcircuit has no path to Ground.");
        return new CircuitNetwork(labels, parts);
    }
}

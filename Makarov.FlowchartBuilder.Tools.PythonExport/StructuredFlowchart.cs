using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Tools.PythonExport;

internal static class StructuredFlowchart
{
    // Reduce single-entry sequences, diamonds and pre-test loops. A graph that cannot
    // be completely reduced keeps its original edges in the dispatch generator.
    public static List<PythonLine>? TryBuild(FlowchartAnalysis graph)
    {
        var exit = new Region();
        var regions = graph.Order.ToDictionary(b => b, _ => new Region());
        foreach (var block in graph.Order)
        {
            var region = regions[block];
            if (block.Name == "End")
            {
                region.Body.Add(new(1, "return", block.Id));
                region.SetNext([exit]);
                continue;
            }
            if (block.Name == "If")
                region.Decision = block;
            else if (block.Name != "Begin")
                region.Body.Add(new(1, graph.Statements[block], block.Id));
            region.SetNext(graph.Successors[block].Select(b => regions[b]).ToArray());
        }

        var start = regions[graph.Order[0]];
        bool changed;
        do
        {
            changed = false;
            foreach (var region in regions.Values)
            {
                if (region.Decision is { } decision)
                    changed |= region.FoldDecision(graph.Statements[decision], decision.Id);
                else
                    while (region.JoinNext()) changed = true;
            }
        } while (changed);

        // Python limits static nesting. Deeply nested regions can still use dispatch.
        return start.Next is [var last] && last == exit && start.Body.All(line => line.Indent < 20)
            ? start.Body : null;
    }

    private sealed class Region
    {
        public List<PythonLine> Body { get; } = [];
        public Region[] Next { get; private set; } = [];
        public BlockGlyph? Decision { get; set; }
        private int Incoming { get; set; }

        public void SetNext(Region[] targets)
        {
            foreach (var old in Next) old.Incoming--;
            Next = targets;
            foreach (var target in targets) target.Incoming++;
        }

        private bool OwnsArm(Region target) => target != this && target.Incoming == 1 && target.Next.Length == 1;

        public bool JoinNext()
        {
            if (Next is not [var next] || !OwnsArm(next)) return false;
            Body.AddRange(next.Body);
            var continuation = next.Next;
            next.SetNext([]);
            SetNext(continuation);
            return true;
        }

        public bool FoldDecision(string condition, string blockId)
        {
            var yes = Next[0];
            var no = Next[1];
            bool yesArm = OwnsArm(yes);
            bool noArm = OwnsArm(no);
            if (yes == no && yes != this)
                Fold("if", condition, blockId, null, null, yes);
            else if (yes != no && (yes == this || yesArm && yes.Next[0] == this))
                Fold("while", condition, blockId, yes == this ? null : yes, null, no);
            else if (yes != no && (no == this || noArm && no.Next[0] == this))
                Fold("while", condition, blockId, no == this ? null : no, null, yes, inverted: true);
            else if (yesArm && yes.Next[0] == no)
                Fold("if", condition, blockId, yes, null, no);
            else if (noArm && no.Next[0] == yes)
                Fold("if", condition, blockId, no, null, yes, inverted: true);
            else if (yesArm && noArm && yes.Next[0] == no.Next[0])
                Fold("if", condition, blockId, yes, no, yes.Next[0]);
            else
                return false;
            return true;
        }

        private void Fold(string keyword, string condition, string blockId, Region? first, Region? second,
            Region continuation, bool inverted = false)
        {
            if (inverted) condition = $"not ({condition})";
            // Preserve tuple and yield expressions that were parenthesized in dispatch.
            else if (condition.Contains(',') || condition.StartsWith("yield", StringComparison.Ordinal)) condition = $"({condition})";
            Body.Add(new(1, $"{keyword} {condition}:", blockId));
            AppendSuite(first, blockId);
            if (second != null)
            {
                Body.Add(new(1, "else:", blockId));
                AppendSuite(second, blockId);
            }
            Decision = null;
            SetNext([continuation]);
        }

        private void AppendSuite(Region? arm, string blockId)
        {
            if (arm is not null)
            {
                foreach (var line in arm.Body) Body.Add(line with { Indent = line.Indent + 1 });
                arm.SetNext([]);
            }
            if (arm == null || arm.Body.All(line => line.Text.StartsWith('#')))
                Body.Add(new(2, "pass", blockId));
        }
    }
}

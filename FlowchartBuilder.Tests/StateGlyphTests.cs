using Makarov.FlowchartBuilder;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.Framework.Core.SettingStorages;
using Xunit;

namespace FlowchartBuilder.Tests;

[Collection("Editor")]
public class StateGlyphTests
{
    private static AbstractGlyph Create(string name)
    {
        var core = Core.Initialize(new MemoryStorage());
        var type = core.GlyphTypes.Single(type => GlyphMetadata.Of(type).Name == name);
        return (AbstractGlyph)Activator.CreateInstance(type)!;
    }

    [Fact]
    public void ScreenAdornmentSurvivesSelectionAndLockButIsSkippedInExport()
    {
        var glyph = new CountingAdornment();
        glyph.Draw(null!);
        glyph.Selected = true;
        glyph.Draw(null!);
        glyph.Fixed = true;
        glyph.Draw(null!);
        Assert.Equal(3, glyph.DrawCount);
        glyph.Draw(null!, adornments: false);
        Assert.Equal(3, glyph.DrawCount);
    }

    private sealed class CountingAdornment : AbstractGlyph
    {
        public int DrawCount { get; private set; }
        protected override void DrawAdornment(Makarov.Framework.Graphics.Canvas canvas) => DrawCount++;
    }

    [Fact]
    public void StartAndFinishHaveDistinctMonochromeMarkers()
    {
        var state = (BlockGlyph)Create("State");
        var initial = state.Metadata.FindActiveProperty("IsInitial")!;
        var final = state.Metadata.FindActiveProperty("IsFinal")!;
        using var ordinary = state.CreateThumbnail();
        initial.SetValue(state, true);
        using var start = state.CreateThumbnail();
        initial.SetValue(state, false);
        final.SetValue(state, true);
        using var finish = state.CreateThumbnail();
        Assert.True(Different(ordinary, start));
        Assert.True(Different(ordinary, finish));
        Assert.True(Different(start, finish));

        static bool Different(Bitmap a, Bitmap b) =>
            Enumerable.Range(0, a.Width).Any(x => Enumerable.Range(0, a.Height).Any(y =>
                (a.GetPixel(x, y).GetBrightness() < 0.6f) != (b.GetPixel(x, y).GetBrightness() < 0.6f)));
    }

    [Fact]
    public void SelfTransitionKeepsOrientationAfterClipboardSaveAndUndo()
    {
        var state = (BlockGlyph)Create("State");
        var edge = (ConnectorGlyph)Create("Transition");
        Assert.Same(edge.GetType(), state.DefaultConnectorType);
        Assert.True(edge.AcceptsEndpoint(state));
        Assert.False(edge.AcceptsEndpoint((BlockGlyph)Create("Begin")));
        Assert.True(edge.AllowsSelfConnection);
        Assert.False(new ConnectorGlyph().AllowsSelfConnection);

        state.Caption = "Waiting";
        state.Metadata.FindActiveProperty("IsInitial")!.SetValue(state, true);
        edge.Connect(state, 0, state, 1);
        edge.Caption = "tick";
        var doc = new Document();
        doc.DocumentSheet.AddToBack([state, edge]);
        doc.CommitChange();
        Assert.Null(state.Metadata.FindActiveProperty("IsCurrent"));
        var current = state.GetType().GetProperty("IsCurrent")!;
        current.SetValue(state, true);
        Assert.False(doc.CommitChange());

        state.Selected = true;
        var copied = Document.ReadClipboard(doc.SelectionXml()!);
        Check(copied.Glyphs);
        Assert.DoesNotContain(copied.Glyphs, glyph => glyph.Id == state.Id || glyph.Id == edge.Id);
        string file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mfb");
        try
        {
            doc.Save(file);
            Assert.DoesNotContain("IsCurrent", File.ReadAllText(file));
            Check(Document.Load(file).DocumentSheet.Glyphs);
        }
        finally { File.Delete(file); }
        edge.Caption = "other";
        doc.CommitChange();
        Assert.True(doc.Undo());
        Check(doc.DocumentSheet.Glyphs);

        void Check(IReadOnlyList<AbstractGlyph> glyphs)
        {
            var restored = Assert.Single(glyphs.OfType<BlockGlyph>());
            var loop = Assert.Single(glyphs.OfType<ConnectorGlyph>());
            Assert.Equal(true, restored.Metadata.FindActiveProperty("IsInitial")!.GetValue(restored));
            Assert.Equal(false, current.GetValue(restored));
            Assert.Same(restored, loop.Source);
            Assert.Same(restored, loop.Target);
            Assert.Equal("tick", loop.Caption);
            Assert.Equal((0, 1), (loop.SourcePort, loop.TargetPort));
        }
    }

    [Fact]
    public void EveryDistinctPairOfStatePortsRoutesSelfTransitionsOutsideTheState()
    {
        var state = (BlockGlyph)Create("State");
        var loop = (ConnectorGlyph)Create("Transition");
        loop.Routing = ConnectorRouting.Straight;
        Assert.Equal(ConnectorRouting.Straight, loop.RoutingBetween(state, (BlockGlyph)Create("State")));
        var bounds = state.RoutingBounds(scaled: false);
        var interior = RectangleF.Inflate(bounds, -0.5f, -0.5f);
        for (int from = 0; from < 4; from++)
        for (int to = 0; to < 4; to++)
        {
            if (from == to) continue;
            var route = ConnectorRoute.Create(loop.RoutingBetween(state, state),
                state.PortPosition(from, false), state.PortDirection(from), bounds,
                state.PortPosition(to, false), state.PortDirection(to), bounds, 16);
            Assert.Equal(state.PortPosition(from, false), route.Start);
            Assert.Equal(state.PortPosition(to, false), route.End);
            Assert.False(interior.Contains(route.Midpoint));
            for (int index = 1; index < route.Polyline.Count; index++)
            {
                var a = route.Polyline[index - 1];
                var b = route.Polyline[index];
                Assert.True(a.X == b.X || a.Y == b.Y);
                var segment = RectangleF.FromLTRB(Math.Min(a.X,b.X), Math.Min(a.Y,b.Y), Math.Max(a.X,b.X), Math.Max(a.Y,b.Y));
                Assert.False(interior.IntersectsWith(RectangleF.Inflate(segment, 0.1f, 0.1f)));
            }
        }
    }
}

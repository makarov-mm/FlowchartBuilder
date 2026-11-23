using Makarov.FlowchartBuilder;
using Makarov.FlowchartBuilder.Glyphs.States;
using Makarov.FlowchartBuilder.Tools.StateMachine;
using Makarov.Framework.Core.SettingStorages;
using Xunit;

namespace FlowchartBuilder.Tests;

[Collection("Editor")]
public sealed class MachineWindowTests
{
    [Fact]
    public async Task BookLoanButtonsDriveTheCanvasMarkerAndEditingRefreshesTheRun()
    {
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                ExerciseWindow();
                completed.SetResult();
            }
            catch (Exception error) { completed.SetException(error); }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(30));
    }

    private static void ExerciseWindow()
    {
        Core.Initialize(new MemoryStorage());
        var document = Document.LoadExample(Path.Combine(AppContext.BaseDirectory, "Examples", "Book-loan.mfb"));
        using var window = new MachineForm(document, "Russian", "Диаграмма состояний…") { Opacity = 0 };
        window.Show();
        Application.DoEvents();

        IEnumerable<StateGlyph> States() => document.DocumentSheet.Glyphs.OfType<StateGlyph>();
        string Highlight() => Assert.Single(States(), s => s.IsCurrent).Id;
        Button Button(string text) => Descendants(window).OfType<Button>().Single(b => b.Text == text && b.Visible);
        var history = Descendants(window).OfType<ListBox>().Single();

        Assert.Equal("active", Highlight());
        Button("Истёк срок").PerformClick();
        Assert.Equal("overdue", Highlight());
        Button("Книгу вернули").PerformClick();
        Assert.Equal("closed", Highlight());
        Assert.Equal(2, history.Items.Count);
        Assert.DoesNotContain(Descendants(window).OfType<Button>(), b => b.Visible && b.Text == "Книгу вернули");
        Button("Начать заново").PerformClick();
        Assert.Equal("active", Highlight());
        Assert.Empty(history.Items.Cast<object>());
        Button("Книгу вернули").PerformClick();
        Assert.Equal("closed", Highlight());
        Assert.False(document.IsModified);

        document.DocumentSheet.Glyphs.Single(g => g.Id == "expire").Caption = "Книгу вернули";
        Assert.True(document.CommitChange());
        Assert.DoesNotContain(States(), s => s.IsCurrent);
        var diagnostics = Descendants(window).OfType<DataGridView>().Single();
        Assert.True(diagnostics.Visible);
        Assert.NotEmpty(diagnostics.Rows.Cast<DataGridViewRow>());
        Assert.True(document.Undo());
        Assert.False(diagnostics.Visible);
        Assert.Equal("active", Highlight());

        window.Close();
        Assert.DoesNotContain(States(), s => s.IsCurrent);
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var nested in Descendants(child)) yield return nested;
        }
    }
}

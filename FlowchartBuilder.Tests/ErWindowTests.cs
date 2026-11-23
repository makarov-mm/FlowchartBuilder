using Makarov.FlowchartBuilder;
using Makarov.FlowchartBuilder.Glyphs.EntityRelationship;
using Makarov.FlowchartBuilder.Tools.EntityRelationship;
using Makarov.Framework.Core.SettingStorages;
using Xunit;

namespace FlowchartBuilder.Tests;

[Collection("Editor")]
public sealed class ErWindowTests
{
    [Fact]
    public async Task QueryTabRunsLibraryPresetsAndPreservesDataWhenATableMoves()
    {
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try { ExerciseWindow(); completed.SetResult(); }
            catch (Exception error) { completed.SetException(error); }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(30));
    }

    private static void ExerciseWindow()
    {
        Core.Initialize(new MemoryStorage());
        var document = Document.LoadExample(Path.Combine(AppContext.BaseDirectory, "Examples", "Library-database.mfb"));
        using var window = new EntityDiagramForm(document, "Russian", "ER-диаграмма → SQLite…") { Opacity = 0 };
        window.Show();
        Application.DoEvents();
        var tabs = Children(window).OfType<TabControl>().Single();
        Assert.Contains("CREATE UNIQUE INDEX", Children(tabs.SelectedTab!).OfType<TextBox>().Single().Text);
        tabs.SelectedIndex = 1;
        var controls = Children(tabs.SelectedTab!).ToArray();
        var presets = controls.OfType<ComboBox>().Single(c => c.AccessibleName == "Шаблоны запросов");
        var grid = controls.OfType<DataGridView>().Single();
        Button Button(string caption) => controls.OfType<Button>().Single(b => b.Text == caption);
        void Run(string name)
        {
            presets.SelectedItem = presets.Items.Cast<object>().Single(item => item.ToString() == name);
            Button("Выполнить SQL").PerformClick();
        }
        Run("Выдать экземпляр 1 Анне");
        Assert.Equal(1, grid.Rows.Count);
        var table = document.DocumentSheet.Glyphs.OfType<TableGlyph>().Single(t => t.Caption == "copies");
        table.XInMM += 10;
        Assert.True(document.CommitChange());
        Run("Выдать экземпляр 1 Олегу");
        Assert.Contains(controls.OfType<Label>(), l => l.Text.Contains("UNIQUE"));
        Run("Выдать экземпляр 2 Олегу");
        Assert.Equal(2, grid.Rows.Count);
        Button("Сбросить данные").PerformClick();
        Run("Посмотреть выдачи");
        Assert.Equal(0, grid.Rows.Count);
    }

    private static IEnumerable<Control> Children(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var nested in Children(child)) yield return nested;
        }
    }
}

using Makarov.FlowchartBuilder.Tools.EntityRelationship;
using Xunit;

namespace FlowchartBuilder.Tests;

[Collection("Editor")]
public sealed class ErSandboxDraftTests
{
    [Fact]
    public async Task CustomQueryAndResultSurvivePendingSchemaAndUndo()
    {
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try { CheckDraft(); completed.SetResult(); }
            catch (Exception error) { completed.SetException(error); }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(30));
    }

    private static void CheckDraft()
    {
        const string original = "CREATE TABLE copies (id INTEGER PRIMARY KEY);";
        const string changed = original + "CREATE TABLE readers (id INTEGER PRIMARY KEY);";
        using var window = new Form { Opacity = 0 };
        using var panel = new SandboxPanel("Russian");
        window.Controls.Add(panel);
        window.Show();
        var layout = Assert.IsType<TableLayoutPanel>(panel.Controls[0]);
        var query = layout.Controls.OfType<TextBox>().Single();
        var presets = layout.Controls.OfType<ComboBox>().Single();
        var grid = layout.Controls.OfType<DataGridView>().Single();
        var buttons = layout.Controls.OfType<FlowLayoutPanel>().Single().Controls.OfType<Button>();
        var run = buttons.Single(b => b.Text == "Выполнить SQL");
        var rebuild = buttons.Single(b => b != run);

        var entry = layout.Controls.Find("addRow", searchAllChildren: true).OfType<Button>().Single();
        panel.SetSchema(original);
        Assert.True(entry.Enabled);
        query.Text = "INSERT INTO copies VALUES(1); SELECT id FROM copies;";
        run.PerformClick();
        Assert.Equal("1", grid[0, 0].Value);
        const string draft = "SELECT count(*) AS n FROM copies;";
        query.Text = draft;
        panel.SetSchema(changed);
        Assert.Equal(draft, query.Text);
        Assert.False(run.Enabled);
        Assert.False(entry.Enabled);
        Assert.Equal("Пересоздать базу", rebuild.Text);
        Assert.Equal("1", grid[0, 0].Value);

        panel.SetSchema(original);
        Assert.True(run.Enabled);
        Assert.True(entry.Enabled);
        run.PerformClick();
        Assert.Equal("1", grid[0, 0].Value);
        panel.SetSchema(changed);
        rebuild.PerformClick();
        Assert.Equal(draft, query.Text);
        Assert.Null(presets.SelectedItem);
        Assert.True(run.Enabled);
        Assert.Contains(presets.Items.Cast<object>(), item => item.ToString() == "Строки таблицы readers");
        run.PerformClick();
        Assert.Equal("0", grid[0, 0].Value);
    }
}

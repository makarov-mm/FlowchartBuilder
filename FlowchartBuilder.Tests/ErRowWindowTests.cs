using System.Runtime.ExceptionServices;
using Makarov.FlowchartBuilder;
using Makarov.FlowchartBuilder.Tools.EntityRelationship;
using Makarov.Framework.Core.SettingStorages;
using Xunit;

namespace FlowchartBuilder.Tests;

[Collection("Editor")]
public sealed class ErRowWindowTests
{
    [Fact]
    public async Task LibraryEntryPreservesDraftAndAllowsCorrectingARejectedLoan()
    {
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try { ExerciseReaderEntry(); completed.SetResult(); }
            catch (Exception error) { completed.SetException(error); }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(30));
    }

    private static void ExerciseReaderEntry()
    {
        Core.Initialize(new MemoryStorage());
        var document = Document.LoadExample(Path.Combine(AppContext.BaseDirectory, "Examples", "Library-database.mfb"));
        using var window = new EntityDiagramForm(document, "Russian", "ER-диаграмма → SQLite…") { Opacity = 0 };
        window.Show();
        var tabs = Children(window).OfType<TabControl>().Single();
        tabs.SelectedIndex = 1;
        var controls = Children(tabs.SelectedTab!).ToArray();
        var table = controls.OfType<ComboBox>().Single(c => c.Name == "entryTable");
        var open = controls.OfType<Button>().Single(b => b.Name == "addRow");
        var query = controls.OfType<TextBox>().Single(c => c.AccessibleName == "SQL-запрос");
        var results = controls.OfType<DataGridView>().Single();
        table.SelectedItem = "readers";
        Assert.Equal("readers", table.SelectedItem);
        // The initial library preset lends a book. Adding a reader must never execute that draft.
        string draft = query.Text;
        Assert.Contains("INSERT INTO loans", draft);

        void OpenEntry(Action<RowEntryForm> edit)
        {
            Exception? callbackError = null;
            bool visited = false;
            window.BeginInvoke(new Action(() =>
            {
                var dialog = Application.OpenForms.OfType<RowEntryForm>().SingleOrDefault();
                try
                {
                    Assert.NotNull(dialog);
                    visited = true;
                    edit(dialog);
                }
                catch (Exception ex) { callbackError = ex; }
                finally
                {
                    if (dialog is { IsDisposed: false, Visible: true, DialogResult: DialogResult.None }) dialog.Close();
                }
            }));
            open.PerformClick();
            if (callbackError != null) ExceptionDispatchInfo.Capture(callbackError).Throw();
            Assert.True(visited);
            Assert.Equal(draft, query.Text);
        }

        void EnterReader(string name, bool save) => OpenEntry(dialog =>
        {
            Children(dialog).OfType<TextBox>().Single(c => c.AccessibleName == "name").Text = name;
            (save ? dialog.AcceptButton : dialog.CancelButton)!.PerformClick();
        });

        EnterReader("Отменённый читатель", save: false);
        const string readerName = "Ирина О'Коннор";
        EnterReader(readerName, save: true);
        Assert.Equal(new[] { "id", "name" }, results.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText));
        Assert.Equal(3, results.Rows.Count);
        Assert.Contains(results.Rows.Cast<DataGridViewRow>(), row => Equals(row.Cells[1].Value, readerName));
        Assert.DoesNotContain(results.Rows.Cast<DataGridViewRow>(), row => Equals(row.Cells[1].Value, "Отменённый читатель"));

        table.SelectedItem = "loans";
        void LendCopy(bool correctDuplicate) => OpenEntry(dialog =>
        {
            var fields = Children(dialog).ToArray();
            var reader = fields.OfType<ComboBox>().Single(c => c.AccessibleName == "reader_id");
            var copy = fields.OfType<ComboBox>().Single(c => c.AccessibleName == "copy_id");
            var issue = fields.OfType<TextBox>().Single(c => c.AccessibleName == "issued_on");
            var due = fields.OfType<TextBox>().Single(c => c.AccessibleName == "due_on");
            reader.SelectedItem = reader.Items.Cast<object>().Single(i => i.ToString() == "3 — " + readerName);
            copy.SelectedItem = copy.Items.Cast<object>().Single(i => i.ToString() == "1");
            issue.Text = "2026-11-23";
            due.Text = "2026-12-07";
            dialog.AcceptButton!.PerformClick();
            if (!correctDuplicate) return;
            Assert.Equal(DialogResult.None, dialog.DialogResult);
            Assert.Contains("UNIQUE", fields.OfType<Label>().Single(c => c.Name == "entryStatus").Text);
            Assert.Equal("2026-12-07", due.Text);
            Assert.Equal("3 — " + readerName, reader.SelectedItem?.ToString());
            copy.SelectedItem = copy.Items.Cast<object>().Single(i => i.ToString() == "2");
            dialog.AcceptButton.PerformClick();
        });
        LendCopy(correctDuplicate: false);
        LendCopy(correctDuplicate: true);
        Assert.Equal(2, results.Rows.Count);
        Assert.All(results.Rows.Cast<DataGridViewRow>(), row => Assert.Equal("3", row.Cells[2].Value));
        Assert.False(document.IsModified);
    }

    private static IEnumerable<Control> Children(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var descendant in Children(child)) yield return descendant;
        }
    }
}

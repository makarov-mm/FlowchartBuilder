namespace Makarov.Framework.Core;

/// <summary>
/// Last-chance error report. Closing the dialog terminates the application.
/// </summary>
public partial class CriticalErrorDialog : Form
{
    public CriticalErrorDialog(Exception exception)
    {
        InitializeComponent();

        lblException.Text = $"Exception: {exception.GetType().Name}";
        tbExceptionDetails.Text = string.Join(
            Environment.NewLine,
            "Message:",
            exception.Message,
            string.Empty,
            "Stack trace:",
            exception.StackTrace);
    }

    private void btnClose_Click(object? sender, EventArgs e) => Environment.Exit(1);

    private void btnCopy_Click(object? sender, EventArgs e) =>
        Clipboard.SetText($"{lblException.Text}{Environment.NewLine}{tbExceptionDetails.Text}");
}

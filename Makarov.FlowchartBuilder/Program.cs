using Makarov.Framework.Core;
using Makarov.Framework.Instance;

namespace Makarov.FlowchartBuilder;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        using SingleInstance? instance = SingleInstance.TryAcquire(Settings.Environment.ApplicationName);
        if (instance is null)
        {
            // Another instance is running: let it open the files and bring itself to the front.
            SingleInstance.TrySendToPrimary(Settings.Environment.ApplicationName, args);
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.ThreadException += (_, e) => ShowCriticalError(e.Exception);

        try
        {
            using Core core = Core.Instance;
            var mainWindow = core.MainWindow;
            mainWindow.HandleCreated += (_, _) => instance.StartListening(new AppInstance(mainWindow));

            core.Run(args);
        }
        catch (Exception e)
        {
            ShowCriticalError(e);
        }
    }

    private static void ShowCriticalError(Exception exception)
    {
        using var dialog = new CriticalErrorDialog(exception);
        dialog.ShowDialog();
    }
}

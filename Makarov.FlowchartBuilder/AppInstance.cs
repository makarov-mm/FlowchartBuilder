using Makarov.FlowchartBuilder.Commands;
using Makarov.FlowchartBuilder.Forms;
using Makarov.Framework.Instance;

namespace Makarov.FlowchartBuilder;

/// <summary>
/// Handles a launch of a second copy of the application: opens the files passed to it
/// (or a new document) in this instance and brings the main window to the front.
/// </summary>
internal sealed class AppInstance(MainForm mainWindow) : IInstance
{
    public void Message(string[] messages)
    {
        try
        {
            mainWindow.BeginInvoke(new Action(() => Open(messages)));
        }
        catch (InvalidOperationException)
        {
            // The main window is closing; there is nobody left to open the files.
        }
    }

    private void Open(string[] fileNames)
    {
        if (mainWindow.WindowState == FormWindowState.Minimized)
            mainWindow.WindowState = mainWindow.WindowStateBeforeMinimize;

        mainWindow.Activate();
        mainWindow.BringToFront();

        if (fileNames.Length == 0)
        {
            Command.GetInstance<NewCommand>().Run();
            return;
        }

        foreach (string fileName in fileNames)
            Core.Instance.TryOpenDocument(fileName);
    }
}

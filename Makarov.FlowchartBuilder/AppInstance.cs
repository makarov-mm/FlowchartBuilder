using System;
using System.Windows.Forms;
using Makarov.Framework.Instance;

namespace Makarov.FlowchartBuilder
{
    public sealed class AppInstance : IInstance
    {
        public void Message(string[] messages)
        {
            var mainWnd = Core.Instance.MainWindow;
            if (mainWnd != null && mainWnd.InvokeRequired)
            {
                mainWnd.BeginInvoke(new Action(() => Message(messages)));
                return;
            }

            if (messages != null && messages.Length > 0)
            {
                foreach (string message in messages)
                    Core.Instance.AddDocument(new Document(message));
            }
            else
            {
                Core.Instance.AddDocument(new Document());
            }

            UpdateWindowState();
        }

        private static void UpdateWindowState()
        {
            var mainWnd = Core.Instance.MainWindow;
            if (mainWnd == null)
                return;

            if (mainWnd.WindowState == FormWindowState.Minimized)
            {
                mainWnd.WindowState = mainWnd.OldWindowState == FormWindowState.Normal
                    ? FormWindowState.Normal
                    : FormWindowState.Maximized;
            }

            mainWnd.Activate();
            mainWnd.BringToFront();
        }
    }
}

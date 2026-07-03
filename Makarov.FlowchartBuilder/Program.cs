using System;
using System.Threading;
using System.Windows.Forms;
using Makarov.Framework.Core;
using Makarov.Framework.Instance;

namespace Makarov.FlowchartBuilder
{
    public sealed class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                var appInstance = new AppInstance();
                bool primary = InstanceHelper.TryStartPrimary(
                    Settings.Environment.ApplicationName, appInstance);

                if (!primary)
                {
                    InstanceHelper.SendToPrimary(
                        Settings.Environment.ApplicationName,
                        args.Length == 0 ? Array.Empty<string>() : args);
                    return;
                }

                Application.ThreadException += Application_ThreadException;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                using (Core.Instance)
                {
                    Core.Instance.Run(args);
                }

                InstanceHelper.Stop();
            }
            catch (Exception e)
            {
                ShowErrorMessage(e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ShowErrorMessage(e.Exception.GetType().Name, e.Exception.Message, e.Exception.StackTrace);
        }

        private static void ShowErrorMessage(string exception, string message, string stackTrace)
        {
            new CriticalErrorDialog(exception, message, stackTrace,
                Settings.Environment.ErrorReportMail,
                Settings.Environment.ErrorReportTitle).ShowDialog();
        }
    }
}

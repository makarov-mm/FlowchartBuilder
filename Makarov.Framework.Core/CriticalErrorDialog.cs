using System;
using System.Windows.Forms;

namespace Makarov.Framework.Core
{
    public partial class CriticalErrorDialog : Form
    {
        public CriticalErrorDialog(string exception, string message, string stackTrace,
            string errorReportMail, string errorReportTitle)
        {
            InitializeComponent();

            ErrorReportMail = errorReportMail;
            ErrorReportTitle = errorReportTitle;

            lblException.Text = $@"Exception: {exception}";
            tbExceptionDetails.Text = string.Format(
                @"Message:{0}{1}{0}{0}Stack trace:{0}{2}",
                Environment.NewLine,
                message,
                stackTrace);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Environment.Exit(1);
            Close();
        }

        private void btnSendReport_Click(object sender, EventArgs e)
        {
            if (!SendReport.Send(ErrorReportMail, ErrorReportTitle, tbExceptionDetails.Text))
            {

            }
        }

        public string ErrorReportMail
        {
            get;
        }

        public string ErrorReportTitle
        {
            get;
        }
    }
}

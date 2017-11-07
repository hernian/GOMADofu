using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GOCalendarSync
{
    public partial class ConfigForm : Form
    {
        private const string NEWLINE = "\r\n";

        public ConfigForm()
        {
            InitializeComponent();
        }

        public string GoogleCalendarID
        {
            get { return textBoxGoogleCalendarID.Text; }
            set { textBoxGoogleCalendarID.Text = value; }
        }

        public bool ProxyAuthenticationEnabled
        {
            get { return checkBoxUseProxyAuthentication.Checked; }
            set { checkBoxUseProxyAuthentication.Checked = value; }
        }

        public string ProxyAccount
        {
            get { return textBoxProxyAccount.Text; }
            set { textBoxProxyAccount.Text = value; }
        }

        public string ProxyPassword
        {
            get { return textBoxProxyPassword.Text; }
            set { textBoxProxyPassword.Text = value; }
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            ExceptionHarness(() =>
            {
                ClearLog();
                AppendLog("接続のテスト中...\r\n");
                var googleCalendarID = textBoxGoogleCalendarID.Text;
                var proxyAccout = textBoxProxyAccount.Text;
                var proxyPassword = textBoxProxyPassword.Text;
                using (var proxyMgr = new ProxyManager(proxyAccout, proxyPassword))
                {
                    proxyMgr.Apply();
                    var googleCalendarProvider = new GoogleCalendarProvider(googleCalendarID);
                    DateTime start = DateTime.Today;
                    DateTime end = start.AddDays(1);
                    googleCalendarProvider.GetRange(start, end);
                }
                AppendLog("正常に接続できました。\r\n");
            });
        }

        private void ClearLog()
        {
            textBoxLog.Text = string.Empty;
        }

        private void AppendLog(string msg)
        {
            var prevLength = textBoxLog.TextLength;
            textBoxLog.Select(prevLength, 0);
            textBoxLog.SelectedText = msg;
            var postLength = textBoxLog.TextLength;
            textBoxLog.Select(postLength, 0);
            textBoxLog.ScrollToCaret();
        }

        private void AppendLog(Exception ex)
        {
            while (ex != null)
            {
                var msg = ex.Message;
                AppendLog(msg);
                if (msg.EndsWith(NEWLINE) == false)
                {
                    AppendLog(NEWLINE);
                }
                ex = ex.InnerException;
            }
        }

        public void ExceptionHarness(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                AppendLog(ex);
            }
        }

        
    }
}

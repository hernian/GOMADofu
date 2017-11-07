using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using GOCalendarSyncCommon;

namespace SyncCalendar
{
    class Program
    {
        private const int SYNC_YEAR_RANGE = 3;     // 3年

        static int Main(string[] args)
        {
            CommandLineOptions options = new CommandLineOptions(args);

            if (options.Debug)
            {
                var c = System.Diagnostics.Process.GetCurrentProcess();
                var msg = string.Format("SyncCalendar process id:{0}", c.Id);
                MessageBox.Show(msg, "SyncCalendar", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            try
            {
                switch (options.Action)
                {
                    case CommandLineOptions.ActionTypes.Regist:
                        RegistPath();
                        break;
                    case CommandLineOptions.ActionTypes.Config:
                        Config();
                        break;
                    case CommandLineOptions.ActionTypes.Sync:
                        SyncCalendar();
                        break;
                }
            }
            catch (Exception ex)
            {
                TraceLog.TheInstance.SetLevel(0).TimeStamp().Tag("SyncCalendar").WriteLine(ex);
            }

            return 0;
        }

        private static void RegistPath()
        {
            var regKey = Registry.CurrentUser.CreateSubKey(@"Software\FCT\GOCalendarSync");
            var myAsm = Assembly.GetExecutingAssembly();
            regKey.SetValue("SyncCalendar", myAsm.Location);
        }

        private static void Config()
        {
            var config = Configuration.Load();
            var form = new ConfigForm();
            form.GoogleCalendarID = config.GoogleCalendarID;
            form.ProxyAccount = config.ProxyAccount;
            form.ProxyPassword = config.ProxyPassword;
            var r = form.ShowDialog();
            if (r == DialogResult.OK)
            {
                config.GoogleCalendarID = form.GoogleCalendarID;
                config.ProxyAccount = form.ProxyAccount;
                config.ProxyPassword = form.ProxyPassword;
                config.Save();
            }
        }

        private static void SyncCalendar()
        {
            using (Stream output = Console.OpenStandardOutput())
            using (Stream input = Console.OpenStandardInput())
            {
                var config = Configuration.Load();
                using (var proxyManager = new ProxyManager(config.ProxyAccount, config.ProxyPassword))
                {
                    proxyManager.Apply();

                    var outlookClient = new GOCalendarSyncCommon.Client(output, input);    // Clientのクラス名前が有りがちなのでフルで指定
                    var lastModified = DateTime.Now;
                    var outlookCalendarProvider = new OutlookCalendarProvider(outlookClient);
                    var googleCalendarProvider = new GoogleCalendarProvider(config.GoogleCalendarID);

                    var syncTime = DateTime.Now;
                    var start = DateTime.Today;
                    var end = start.AddYears(SYNC_YEAR_RANGE);
                    var outlookCalendarItems = outlookCalendarProvider.GetRange(start, end);
                    var googleCalendarItems = googleCalendarProvider.GetRange(start, end);

                    var synchronizer = new CalendarItemSynchronizer(googleCalendarItems, outlookCalendarItems);
                    synchronizer.Synchronize();

                    outlookCalendarProvider.Apply(outlookCalendarItems, lastModified);
                    googleCalendarProvider.Apply(googleCalendarItems);
                }
            }
        }
    }
}

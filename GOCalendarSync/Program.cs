using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;
using GOCalendarSyncCommon;

namespace GOCalendarSync
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>/
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var options = new CommandLineOptions(args);
            if (options.Debug)
            {
                ShowProcessID();
            }
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            switch (options.ActionType)
            {
                case CommandLineOptions.ActionTypes.Config:
                    Configure();
                    break;
                case CommandLineOptions.ActionTypes.Regist:
                    Regist();
                    break;
                case CommandLineOptions.ActionTypes.Sync:
                    Sync();
                    break;
                default:
                    break;
            }
        }

        private static void ShowProcessID()
        {
            var execAsm = Assembly.GetExecutingAssembly();
            var asmName = execAsm.GetName();
            var currentProcess = Process.GetCurrentProcess();
            var msg = string.Format("process id:{0}", currentProcess.Id);
            MessageBox.Show(msg, asmName.Name, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private static void Configure()
        {
            var config = Config.Load();
            var form = new ConfigForm();
            form.GoogleCalendarID = config.GoogleCalendarID;
            form.ProxyAuthenticationEnabled = config.ProxyAuthenticationEnabled;
            form.ProxyAccount = config.ProxyAccount;
            form.ProxyPassword = config.ProxyPassword;
            var r = form.ShowDialog();
            if (r == DialogResult.OK)
            {
                config.GoogleCalendarID = form.GoogleCalendarID;
                config.ProxyAuthenticationEnabled = form.ProxyAuthenticationEnabled;
                config.ProxyAccount = form.ProxyAccount;
                config.ProxyPassword = form.ProxyPassword;
                config.Save();
            }
        }

        private static void Regist()
        {
            var myAsm = Assembly.GetExecutingAssembly();
            var regKey = Registry.CurrentUser.CreateSubKey(Constants.REG_KEY_SYNC_EXE);
            regKey.SetValue(null, myAsm.Location);
        }

        private static void Sync()
        {
            var form = new SyncForm(SyncCalendar);
            Application.Run();
            form.Close();
        }

        private static void SyncCalendar()
        {
            using (Stream output = Console.OpenStandardOutput())
            using (Stream input = Console.OpenStandardInput())
            {
                var config = Config.Load();
                using (var proxyManager = new ProxyManager(config.ProxyAccount, config.ProxyPassword))
                {
                    proxyManager.Apply();

                    var outlookClient = new GOCalendarSyncCommon.Client(output, input);    // Clientのクラス名前が有りがちなのでフルで指定
                    var outlookCalendarProvider = new OutlookCalendarProvider(outlookClient);
                    var googleCalendarProvider = new GoogleCalendarProvider(config.GoogleCalendarID);

                    var syncTime = DateTime.Now;
                    var start = DateTime.Today;
                    var end = start.AddYears(Constants.SYNC_YEAR_RANGE);
                    var outlookCalendarItems = outlookCalendarProvider.GetRange(start, end);
                    var googleCalendarItems = googleCalendarProvider.GetRange(start, end);

                    var synchronizer = new CalendarItemSynchronizer(googleCalendarItems, outlookCalendarItems);
                    synchronizer.Synchronize();

                    // 先にGoogleカレンダーを更新する
                    // もし先にOutlookカレンダーを更新すると、Outlookで削除された項目がGoogleに追加するより前に削除されたと誤認してしまう。
                    // OutlookカレンダーをGoogleカレンダーより1秒後に更新することで、Outlookで項目が削除されときにGoogleへの追加より後だと認識できるようにする。
                    googleCalendarProvider.Apply(googleCalendarItems);
                    System.Threading.Thread.Sleep(1000);
                    var lastModified = DateTime.Now;
                    outlookCalendarProvider.Apply(outlookCalendarItems, lastModified);
                }
            }
        }
    }
}

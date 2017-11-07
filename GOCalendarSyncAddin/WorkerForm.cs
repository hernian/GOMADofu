using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Outlook = Microsoft.Office.Interop.Outlook;
using GOCalendarSyncCommon;

namespace GOCalendarSyncAddin
{
    public partial class WorkerForm : Form
    {
        const int WM_POWERBORADCAST = 0x218;
        const int PBT_APMSUSPEND = 0x4;
        const int PBT_APMRESUMESUSPEND = 0x07;

        private const string USER_PROPERTY_NAME_GO_CALENDAR_SYNC = "GOMADofuGUID";
        private const string TAG_LOG = "Outlook";
        private const string SYNC_EXE_OPTION_CONFIGURE = "/config";
        private const string SYNC_EXE_OPTION_SYNCHRONIZE = "/sync";

        private Outlook.Application _outlookApp;
        private GOCalendarSyncCommon.Server _server = null;
        private AppointmentItemCollection _appointmentItems = new AppointmentItemCollection();
        private volatile IntPtr _hWnd;
        private Icon[] _icons = { Properties.Resources.NotifyIcon, Properties.Resources.NotifyIcon2 };
        private int _iconIndex = 0;

        public WorkerForm(Outlook.Application outlookApp)
        {
            InitializeComponent();

            _outlookApp = outlookApp;

            // ここでthis.Handleをアクセスしてウィンドウハンドルを生成しないと、後のInvokeRequiredが期待通り動かない
            // _hWndの値が後で必要になるわけではない。
            // ただし、参照しないメンバ変数があると警告がでるので、警告除けのために HWnd プロパティを作った。
            _hWnd = this.Handle;

            timerAutoSync.Start();
            UpdateToolStripMenuItemState();
        }

        public IntPtr HWnd
        {
            get { return _hWnd;  }
        }

        public void StartConfigration()
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = GetSyncExePathName();
            startInfo.Arguments = SYNC_EXE_OPTION_CONFIGURE;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.Start();
            UpdateToolStripMenuItemState();
        }

        public void StartSynchronizing()
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = GetSyncExePathName();
            startInfo.Arguments = SYNC_EXE_OPTION_SYNCHRONIZE;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            process.StartInfo = startInfo;
            var r = process.Start();
            if (r)
            {
                var stdout = process.StandardOutput.BaseStream;
                var stdin = process.StandardInput.BaseStream;
                _server = new GOCalendarSyncCommon.Server(stdout, stdin, server_HandlePacket);
                _server.Start();
                timerBlink.Start();
            }
            UpdateToolStripMenuItemState();
        }

        public void CloseForm()
        {
            if (_server != null)
            {
                _server.Dispose();
                _server = null;
            }
            process.Close();
            this.Close();
        }

        private void toolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            ExceptionHarness(() =>
            {
                var myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = myAssembly.GetName().Version;
                var sb = new StringBuilder();
                sb.Append(Constants.NAME_APP).AppendLine().Append(version);
                MessageBox.Show(sb.ToString(), Constants.NAME_APP, MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void toolStripMenuItemSettings_Click(object sender, EventArgs e)
        {
            ExceptionHarness(() =>
            {
                StartConfigration();
            });

        }

        private void toolStripMenuItemSyncNow_Click(object sender, EventArgs e)
        {
            ExceptionHarness(() =>
            {
                StartSynchronizing();
            });

        }

        private void toolStripMenuItemShowLog_Click(object sender, EventArgs e)
        {
            ExceptionHarness(() =>
            {
                var p = Process.Start(TraceLog.TheInstance.LogFileName);
                if (p != null)
                {
                    p.Close();
                }
            });
        }

        private void process_Exited(object sender, EventArgs e)
        {
            HandleProcessExited();
        }

        private void HandleProcessExited()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(HandleProcessExited));
                return;
            }

            if (_server != null)
            {
                _server.Dispose();
                _server = null;
            }
            UpdateToolStripMenuItemState();

            timerBlink.Stop();
            _iconIndex = 0;
            notifyIcon.Icon = _icons[_iconIndex];
        }

        private void UpdateToolStripMenuItemState()
        {
            if (SafeProcessHasExited())
            {
                toolStripMenuItemSettings.Enabled = true;
                toolStripMenuItemSyncNow.Enabled = true;
            }
            else
            {
                toolStripMenuItemSettings.Enabled = false;
                toolStripMenuItemSyncNow.Enabled = false;
            }
        }

        private bool SafeProcessHasExited()
        {
            var result = false;
            try
            {
                result = process.HasExited;
            }
            catch (InvalidOperationException)
            {
                result = true;
            }
            return result;
        }

        private Packet server_HandlePacket(Packet request)
        {
            var response = default(Packet);
            if (InvokeRequired)
            {
                response = (Packet)Invoke(new Server.PacketHandler(server_HandlePacket), new object[] { request });
            }
            else{
                if (request is GetRangeRequest)
                {
                    response = GetRange((GetRangeRequest)request);
                }
                else if (request is ApplyRequest)
                {
                    response = Apply((ApplyRequest)request);
                }
                else
                {
                    response = new Response() { ErrorMessage = "不明なリクエスト" };
                }
            }
            return response;
        }

        private Packet GetRange(GetRangeRequest req)
        {
            var res = new GetRangeResponse();
            try
            {
                var calendarItems = new CalendarItemCollection();
                var calendarFolder = _outlookApp.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar);
                var filter = string.Format("[End] > '{0}' and [Start] < '{1}'", req.Start.ToString("g"), req.End.ToString("g"));
                var appoItems = calendarFolder.Items.Restrict(filter);
                foreach (Outlook.AppointmentItem appoItem in appoItems)
                {
                    if (appoItem.RecurrenceState != Outlook.OlRecurrenceState.olApptNotRecurring)
                    {
                        continue;
                    }
                    // filterでうまくフィルタできなかったことがあるので、ここで再チェックする
                    if ((appoItem.End >= req.Start) && (appoItem.Start < req.End)) 
                    {
                        _appointmentItems.Add(appoItem);
                        var calendarItem = CreateCalendarItem(appoItem);
                        calendarItems.Add(calendarItem);
                    }
                }
                res.CalendarItems = calendarItems;
            }
            catch (Exception ex)
            {
                res.ErrorMessage = ex.Message;
            }
            return res;
        }

        private CalendarItem CreateCalendarItem(Outlook.AppointmentItem appoItem)
        {
            var calendarItem = new CalendarItem();
            calendarItem.ID = appoItem.EntryID;
            calendarItem.SyngronizeGuid = GetGOCalendarSyncGUID(appoItem);
            calendarItem.Name = appoItem.Subject;
            calendarItem.AllDayEvent = appoItem.AllDayEvent;
            calendarItem.Cancelled = false;
            calendarItem.Start = appoItem.Start;
            calendarItem.End = appoItem.End;
            calendarItem.LastModified = appoItem.LastModificationTime;
            calendarItem.Location = appoItem.Location;
            calendarItem.Body = appoItem.Body;
            return calendarItem;
        }

        private Guid GetGOCalendarSyncGUID(Outlook.AppointmentItem appoItem)
        {
            var prop = appoItem.UserProperties.Find(USER_PROPERTY_NAME_GO_CALENDAR_SYNC);
            if (prop == null)
            {
                return Guid.Empty;
            }
            if (prop.Value == null)
            {
                return Guid.Empty;
            }
            var strValue = (string)prop.Value;
            var guid = default(Guid);
            var r = Guid.TryParse(strValue, out guid);
            if (r == false)
            {
                return Guid.Empty;
            }
            return guid;
        }

        private Packet Apply(ApplyRequest req)
        {
            Log(0, "Outlook更新開始");
            var res = new ApplyResponse() { CalendarItems = new CalendarItemCollection() };
            foreach (var calendarItem in req.CalendarItems)
            {
                try
                {
                    if (calendarItem.Changed == false)
                    {
                        res.CalendarItems.Add(calendarItem);
                    }
                    else if (calendarItem.Cancelled)
                    {
                        DeleteItem(calendarItem);
                    }
                    else if (calendarItem.ID == string.Empty)
                    {
                        CreateItem(calendarItem);
                        res.CalendarItems.Add(calendarItem);
                    }
                    else
                    {
                        UpdateItem(calendarItem);
                        res.CalendarItems.Add(calendarItem);
                    }
                }
                catch (Exception ex)
                {
                    Log(0, ex);
                }
            }
            Log(0, "Outlook更新完了");
            return res;
        }

        private void CreateItem(CalendarItem calendarItem)
        {
            Log(0, "新規", calendarItem);
            var calendarFolder = _outlookApp.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar);
            var appoItem = (Outlook.AppointmentItem)calendarFolder.Items.Add();
            UpdateAppointmentItem(appoItem, calendarItem);
            calendarItem.ID = appoItem.EntryID;
            calendarItem.LastModified = appoItem.LastModificationTime;
        }

        private void DeleteItem(CalendarItem calendarItem)
        {
            Log(0, "削除", calendarItem);
            var appoItem = _appointmentItems.FindByEntryID(calendarItem.ID);
            if (appoItem == null)
            {
                // あるはずのアポイントメントアイテムが見つからないので放置する
                return;
            }

            appoItem.Delete();
        }

        private void UpdateItem(CalendarItem calendarItem)
        {
            Log(0, "変更後", calendarItem);
            var appoItem = _appointmentItems.FindByEntryID(calendarItem.ID);
            if (appoItem == null)
            {
                // あるはずのアポイントメントアイテムが見つからないので放置する
                return;
            }

            var orgCalendarItem = CreateCalendarItem(appoItem);
            Log(0, "変更前", orgCalendarItem);
            UpdateAppointmentItem(appoItem, calendarItem);
        }

        private void UpdateAppointmentItem(Outlook.AppointmentItem appoItem, CalendarItem calendarItem)
        {
            if (EqualsAppointmentPropertyString(appoItem.Subject, calendarItem.Name) == false)
            {
                appoItem.Subject = calendarItem.Name;
            }
            if (appoItem.AllDayEvent != calendarItem.AllDayEvent)
            {
                appoItem.AllDayEvent = calendarItem.AllDayEvent;
            }
            if (appoItem.Start != calendarItem.Start)
            {
                appoItem.Start = calendarItem.Start;
            }
            if (appoItem.End != calendarItem.End)
            {
                appoItem.End = calendarItem.End;
            }
            if (EqualsAppointmentPropertyString(appoItem.Location, calendarItem.Location) == false)
            {
                appoItem.Location = calendarItem.Location;
            }
            if (EqualsAppointmentPropertyString(appoItem.Body, calendarItem.Body) == false)
            {
                appoItem.Body = calendarItem.Body;
            }
            var userProp = appoItem.UserProperties[USER_PROPERTY_NAME_GO_CALENDAR_SYNC];
            if (userProp != null)
            {
                var guidStr = calendarItem.SyngronizeGuid.ToString();
                if (userProp.Value != guidStr)
                {
                    userProp.Value = guidStr;
                }
            }
            else
            {
                var p = appoItem.UserProperties.Add(USER_PROPERTY_NAME_GO_CALENDAR_SYNC, Outlook.OlUserPropertyType.olText);
                p.Value = calendarItem.SyngronizeGuid.ToString();
            }
            appoItem.Save();

            var resultCalendarItem = CreateCalendarItem(appoItem);
        }

        private bool EqualsAppointmentPropertyString(string propertyStr, string newValue)
        {
            if (string.IsNullOrEmpty(propertyStr) && string.IsNullOrEmpty(newValue))
            {
                return true;
            }
            return propertyStr == newValue;
        }

        private string GetSyncExePathName()
        {
            var regKey = Registry.CurrentUser.OpenSubKey(Constants.REG_KEY_SYNC_EXE);
            var pathSyncExe = (string)regKey.GetValue(null);
            return pathSyncExe;
        }

        private void ExceptionHarness(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Constants.NAME_APP, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Log(int level, object obj)
        {
            Log(level, obj.ToString());
        }

        private void Log(int level, string logStr)
        {
            TraceLog.TheInstance.Header(level, TAG_LOG).WriteLine(logStr);
        }

        private void Log(int level, string format, params object[] args)
        {
            string logStr = string.Format(format, args);
            Log(level, logStr);
        }

        private void Log(int level, string action, CalendarItem calendarItem)
        {
            string logStr = string.Format("{0} 名前:{1} 開始時刻:{2} 終了時刻:{3} GUID:{4} 場所:{5} 本文:{6}",
                                action,
                                calendarItem.Name,
                                calendarItem.Start.ToString("s"),
                                calendarItem.End.ToString("s"),
                                calendarItem.SyngronizeGuid.ToString("B"),
                                calendarItem.Location,
                                calendarItem.Body);
            Log(level, logStr);
        }

        private void timerBlink_Tick(object sender, EventArgs e)
        {
            ExceptionHarness(() => 
            {
                ++_iconIndex;
                if (_iconIndex >= _icons.Length)
                {
                    _iconIndex = 0;
                }
                notifyIcon.Icon = _icons[_iconIndex];
            });
        }

        private void OnWmPowerBroadcast(Message m)
        {
            int pbt = m.WParam.ToInt32();
            if (pbt == PBT_APMSUSPEND)
            {
                timerAutoSync.Stop();
            }
            else if (pbt == PBT_APMRESUMESUSPEND)
            {
                timerAutoSync.Start();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_POWERBORADCAST)
            {
                OnWmPowerBroadcast(m);
                return;
            }
            base.WndProc(ref m);
        }

        private void timerAutoSync_Tick(object sender, EventArgs e)
        {
            ExceptionHarness(() =>
            {
                var r = SafeProcessHasExited();
                if (r)
                {
                    StartSynchronizing();
                }
            });
        }
    }
}

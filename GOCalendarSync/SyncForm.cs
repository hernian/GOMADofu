using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GOCalendarSyncCommon;

namespace GOCalendarSync
{
    public partial class SyncForm : Form
    {
        public SyncForm(Action syncAction)
        {
            InitializeComponent();

            backgroundWorker.RunWorkerAsync(syncAction);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var syncAction = (Action)e.Argument;
                syncAction();
            }
            catch (Exception ex)
            {
                TraceLog.TheInstance.SetLevel(0).TimeStamp().Tag(Constants.TAG_SYNC_EXE).WriteLine(ex);
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Application.Exit();
        }
    }
}

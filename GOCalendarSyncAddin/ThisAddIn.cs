using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Outlook = Microsoft.Office.Interop.Outlook;
using Office = Microsoft.Office.Core;

namespace GOCalendarSyncAddin
{
    public partial class ThisAddIn
    {
        private WorkerForm _workerForm;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            _workerForm = new WorkerForm(this.Application);
            var appEvent = (Outlook.ApplicationEvents_11_Event)this.Application;
            appEvent.Quit += application_Quit;
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            SafeDispose();
        }

        private void application_Quit()
        {
            SafeDispose();
        }

        private void SafeDispose()
        {
            if (_workerForm != null)
            {
                _workerForm.CloseForm();
                _workerForm = null;
            }
        }

        #region VSTO で生成されたコード

        /// <summary>
        /// デザイナーのサポートに必要なメソッドです。
        /// このメソッドの内容をコード エディターで変更しないでください。
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}

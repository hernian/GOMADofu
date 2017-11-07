namespace GOCalendarSyncAddin
{
    partial class WorkerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkerForm));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSyncNow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemShowLog = new System.Windows.Forms.ToolStripMenuItem();
            this.process = new System.Diagnostics.Process();
            this.timerBlink = new System.Windows.Forms.Timer(this.components);
            this.timerAutoSync = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "GOMADofu";
            this.notifyIcon.Visible = true;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAbout,
            this.toolStripMenuItemSettings,
            this.toolStripMenuItemSyncNow,
            this.toolStripMenuItemShowLog});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(205, 92);
            // 
            // toolStripMenuItemAbout
            // 
            this.toolStripMenuItemAbout.Name = "toolStripMenuItemAbout";
            this.toolStripMenuItemAbout.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemAbout.Text = "GOMADofuについて(&A)...";
            this.toolStripMenuItemAbout.Click += new System.EventHandler(this.toolStripMenuItemAbout_Click);
            // 
            // toolStripMenuItemSettings
            // 
            this.toolStripMenuItemSettings.Name = "toolStripMenuItemSettings";
            this.toolStripMenuItemSettings.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemSettings.Text = "設定(&S)...";
            this.toolStripMenuItemSettings.Click += new System.EventHandler(this.toolStripMenuItemSettings_Click);
            // 
            // toolStripMenuItemSyncNow
            // 
            this.toolStripMenuItemSyncNow.Name = "toolStripMenuItemSyncNow";
            this.toolStripMenuItemSyncNow.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemSyncNow.Text = "今すぐ同期する(&Y)";
            this.toolStripMenuItemSyncNow.Click += new System.EventHandler(this.toolStripMenuItemSyncNow_Click);
            // 
            // toolStripMenuItemShowLog
            // 
            this.toolStripMenuItemShowLog.Name = "toolStripMenuItemShowLog";
            this.toolStripMenuItemShowLog.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemShowLog.Text = "ログを表示する(&L)";
            this.toolStripMenuItemShowLog.Click += new System.EventHandler(this.toolStripMenuItemShowLog_Click);
            // 
            // process
            // 
            this.process.EnableRaisingEvents = true;
            this.process.StartInfo.Domain = "";
            this.process.StartInfo.LoadUserProfile = false;
            this.process.StartInfo.Password = null;
            this.process.StartInfo.StandardErrorEncoding = null;
            this.process.StartInfo.StandardOutputEncoding = null;
            this.process.StartInfo.UserName = "";
            this.process.SynchronizingObject = this;
            this.process.Exited += new System.EventHandler(this.process_Exited);
            // 
            // timerBlink
            // 
            this.timerBlink.Interval = 1000;
            this.timerBlink.Tick += new System.EventHandler(this.timerBlink_Tick);
            // 
            // timerAutoSync
            // 
            this.timerAutoSync.Interval = 1800000;
            this.timerAutoSync.Tick += new System.EventHandler(this.timerAutoSync_Tick);
            // 
            // WorkerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(297, 94);
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "WorkerForm";
            this.Text = "GOCalendarSyncAddin";
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAbout;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSettings;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSyncNow;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemShowLog;
        private System.Diagnostics.Process process;
        private System.Windows.Forms.Timer timerBlink;
        private System.Windows.Forms.Timer timerAutoSync;
    }
}
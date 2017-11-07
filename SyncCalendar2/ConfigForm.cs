using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SyncCalendar
{
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();
        }

        public string GoogleCalendarID
        {
            get { return textBoxGoogleCalendarID.Text; }
            set { textBoxGoogleCalendarID.Text = value; }
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
    }
}

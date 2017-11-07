using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace GOCalendarSyncCommon
{
    public class LastCalendarItemsData
    {
        public static string GetVersion()
        {
            var myAssembly = Assembly.GetExecutingAssembly();
            var verInfo =  myAssembly.GetName().Version;
            return verInfo.ToString();
        }

        public string Version = string.Empty;
        public CalendarItemCollection CalendarItems;
        public DateTime LastModified;

        public bool IsValid
        {
            get
            {
                var currentVersion = GetVersion();
                return currentVersion == this.Version;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GOCalendarSyncCommon
{
    public class CalendarItem
    {
        public string ID = string.Empty;
        public Guid SyngronizeGuid = Guid.Empty;
        public string Name = string.Empty;
        public DateTime Start = new DateTime(0L);
        public DateTime End = new DateTime(0L);
        public DateTime LastModified = new DateTime(0L);
        public bool AllDayEvent = false;
        public bool Cancelled = false;
        public string Location = string.Empty;
        public string Body = string.Empty;
        public bool Changed = false;
    }
}

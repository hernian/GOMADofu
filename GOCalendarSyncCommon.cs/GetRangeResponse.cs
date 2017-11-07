using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOCalendarSyncCommon
{
    public class GetRangeResponse : Packet
    {
        public CalendarItemCollection CalendarItems;
        public string ErrorMessage;
    }
}

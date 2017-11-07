using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOCalendarSyncCommon
{
    public class GetRangeRequest : Packet
    {
        public DateTime Start;
        public DateTime End;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GOCalendarSyncCommon
{
    [XmlInclude(typeof(GetRangeRequest))]
    [XmlInclude(typeof(GetRangeResponse))]
    [XmlInclude(typeof(ApplyRequest))]
    [XmlInclude(typeof(ApplyResponse))]
    [XmlInclude(typeof(Response))]
    [XmlInclude(typeof(ErrorResponse))]
    public class Packet
    {
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace GOCalendarSyncAddin
{
    class AppointmentItemCollection
    {
        private Dictionary<string, Outlook.AppointmentItem> _dict = new Dictionary<string,Outlook.AppointmentItem>();

        public AppointmentItemCollection()
        {
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public void Add(Outlook.AppointmentItem appointmentItem)
        {
            _dict[appointmentItem.EntryID] = appointmentItem;
        }

        public Outlook.AppointmentItem FindByEntryID(string entryID)
        {
            var appointmentItem = default(Outlook.AppointmentItem);
            var r = _dict.TryGetValue(entryID, out appointmentItem);
            if (r == false)
            {
                appointmentItem = null;
            }
            return appointmentItem;
        }
    }
}

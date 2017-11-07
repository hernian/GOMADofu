using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOCalendarSyncCommon
{
    public class GatheredCalenderItems
    {
        public enum CalendarItemTypes
        {
            Google,
            Outlook
        }

        private readonly Guid _guid;
        private CalendarItem _key = null;
        private List<CalendarItem> _allCalendarItems = new List<CalendarItem>();
        private List<CalendarItem> _googleCalendarItems = new List<CalendarItem>();
        private List<CalendarItem> _outlookCalendarItems = new List<CalendarItem>();

        public GatheredCalenderItems(Guid guid)
        {
            _guid = guid;
        }

        public GatheredCalenderItems()
        {
            _guid = Guid.NewGuid();
        }

        public Guid Guid
        {
            get { return _guid; }
        }

        public CalendarItem Key
        {
            get { return _key; }
        }

        public void AddItem(CalendarItem calendarItem, CalendarItemTypes type)
        {
            List<CalendarItem> list = (type == CalendarItemTypes.Google) ? _googleCalendarItems : _outlookCalendarItems;
            
            DoAddItem(calendarItem, list);
        }

        private void DoAddItem(CalendarItem calendarItem, List<CalendarItem> list)
        {
            _allCalendarItems.Add(calendarItem);
            list.Add(calendarItem);
            if (_key == null)
            {
                _key = calendarItem;
            }
            else
            {
                if (_key.LastModified < calendarItem.LastModified)
                {
                    _key = calendarItem;
                }
            }
        }

        public IEnumerable<CalendarItem> Items
        {
            get { return _allCalendarItems; }
        }

        public int Count
        {
            get { return _allCalendarItems.Count; }
        }

        public IEnumerable<CalendarItem> GoogleItems
        {
            get { return _googleCalendarItems; }
        }

        public int CountGoogleItems
        {
            get { return _googleCalendarItems.Count; }
        }

        public IEnumerable<CalendarItem> OutlookItems
        {
            get { return _outlookCalendarItems; }
        }

        public int CountOutlookItems
        {
            get { return _outlookCalendarItems.Count; }
        }
    }
}

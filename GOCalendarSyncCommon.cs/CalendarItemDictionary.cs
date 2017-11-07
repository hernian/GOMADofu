using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOCalendarSyncCommon
{
    public class CalendarItemDictionary : Dictionary<string, CalendarItem>
    {
        public delegate string GetKeyHandler(CalendarItem calendarItem);

        private GetKeyHandler _getKeyHandler;

        public CalendarItemDictionary(GetKeyHandler getKeyHandler)
        {
            _getKeyHandler = getKeyHandler;
        }

        public void Add(CalendarItem calendarItem)
        {
            var key = _getKeyHandler(calendarItem);
            this[key] = calendarItem;
        }

        public void AddRange(IEnumerable<CalendarItem> calendarItems)
        {
            foreach (var calendarItem in calendarItems)
            {
                Add(calendarItem);
            }
        }

        public bool ContainsKey(CalendarItem calendarItem)
        {
            var key = _getKeyHandler(calendarItem);
            return this.ContainsKey(key);
        }

        public CalendarItem Find(string key)
        {
            var result = default(CalendarItem);
            var r = this.TryGetValue(key, out result);
            if (r == false)
            {
                result = null;
            }
            return result;
        }
    }
}

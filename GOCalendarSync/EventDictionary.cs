using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace GOCalendarSync
{
    class EventDictionary
    {
        private Dictionary<string, Event> _dict = new Dictionary<string, Event>();

        public EventDictionary()
        {
        }

        public void Add(Event ev)
        {
            _dict[ev.Id] = ev;
        }

        public Event Find(string id)
        {
            var ev = default(Event);
            var r = _dict.TryGetValue(id, out ev);
            if (r == false)
            {
                return null;
            }
            return ev;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using GOCalendarSyncCommon;

namespace SyncCalendar
{
    class OutlookCalendarProvider
    {
        private static string GetLastItemsFileName()
        {
            var pathAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var pathLastItems = Path.Combine(pathAppData, @"FCT\GOCalendarSync\LastOutlookItems.xml");
            return pathLastItems;
        }

        private Encoding _encoding = new UTF8Encoding(false);
        private string _pathLastItems = GetLastItemsFileName();
        private XmlSerializer _serializer = new XmlSerializer(typeof(LastCalendarItemsData));
        private Client _client;

        public OutlookCalendarProvider(Client client)
        {
            _client = client;
        }

        public CalendarItemCollection GetRange(DateTime start, DateTime end)
        {
            var nowItems = _client.GetRange(start, end);
            var lastItems = LoadLastItems();
#if false
            DumpCalendarItemCollection(nowItems, @"d:\temp\nowItems.xml");
            DumpCalendarItemCollection(lastItems, @"d:\temp\lastItems.xml");
#endif
            var nowItemsDict = new CalendarItemDictionary();
            nowItemsDict.AddRange(nowItems);

            foreach (var lastItem in lastItems)
            {
                if (lastItem.Cancelled)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(lastItem.ID))
                {
                    continue;
                }
                if (nowItemsDict.ContainsKey(lastItem.ID) == false)
                {
                    lastItem.Cancelled = true;
                    nowItems.Add(lastItem);
                }
            }

            return nowItems;
        }

        public void Apply(CalendarItemCollection calendarItems, DateTime lastModified)
        {
            CalendarItemCollection results = _client.Apply(calendarItems);
            LastCalendarItemsData lastItemsData = new LastCalendarItemsData();
            lastItemsData.CalendarItems = results;
            lastItemsData.LastModified = lastModified;
            using (var w = new StreamWriter(_pathLastItems, false, _encoding))
            {
                _serializer.Serialize(w, lastItemsData);
            }
        }

        private CalendarItemCollection LoadLastItems()
        {
            var lastItems = default(CalendarItemCollection);
            try
            {
                using (var r = new StreamReader(_pathLastItems, _encoding))
                {
                    var lastItemsData = (LastCalendarItemsData)_serializer.Deserialize(r);
                    foreach (var calendarItem in lastItemsData.CalendarItems)
                    {
                        calendarItem.LastModified = lastItemsData.LastModified;
                        calendarItem.Changed = false;
                    }
                    lastItems = lastItemsData.CalendarItems;
                }
            }
            catch (Exception)
            {
                lastItems = new CalendarItemCollection();
            }
            return lastItems;
        }

#if false
        private void DumpCalendarItemCollection(CalendarItemCollection calendarItems, string fileName)
        {
            var enc = new UTF8Encoding(false);
            using (var w = new StreamWriter(fileName, false, enc))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(CalendarItemCollection));
                serializer.Serialize(w, calendarItems);
            }
        }
#endif
    }
}

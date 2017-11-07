using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GOCalendarSyncCommon
{
    public class CalendarSyncData
    {
        public static CalendarSyncData Restore(string data)
        {
            var result = default(CalendarSyncData);
            using (var reader = new StringReader(data))
            {
                result = Restore(reader);
            }
            return result;
        }

        public static CalendarSyncData Restore(TextReader reader)
        {
            var serializer = new XmlSerializer(typeof(CalendarSyncData));
            var result = (CalendarSyncData)serializer.Deserialize(reader);
            return result;
        }

        public DateTime Start;
        public DateTime End;
        public List<CalendarItem> Items = new List<CalendarItem>();
        public List<string> Errors = new List<string>();

        public CalendarSyncData()
        {
        }

        public void Send(TextWriter writer)
        {
            var serializer = new XmlSerializer(typeof(CalendarSyncData));
            serializer.Serialize(writer, this);

            // デバッグ出力にも書き出す
            StringWriter w = new StringWriter();
            serializer.Serialize(w, this);
            TraceLog.TheInstance.WriteLine(w.ToString());
        }
    }
}

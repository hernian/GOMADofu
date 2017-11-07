using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GOCalendarSyncCommon
{
    public class Client
    {
        private BinaryWriter _writer;
        private BinaryReader _reader;
        private Encoding _encoding;
        private XmlSerializer _serializer;

        public Client(Stream output, Stream input)
        {
            _writer = new BinaryWriter(output);
            _reader = new BinaryReader(input);
            _encoding = Encoding.Unicode;
            _serializer = new XmlSerializer(typeof(Packet));
        }

        public CalendarItemCollection GetRange(DateTime start, DateTime end)
        {
            var req = new GetRangeRequest() { Start = start, End = end };
            var res = (GetRangeResponse)SendReceive(req);
            return res.CalendarItems;
        }

        public CalendarItemCollection Apply(CalendarItemCollection calendarItems)
        {
            var req = new ApplyRequest() { CalendarItems = calendarItems };
#if false
            using (var w = new StreamWriter(@"d:\temp\apply.txt", false, new UTF8Encoding(false)))
            {
                _serializer.Serialize(w, req);
            }
#endif
            var res = (ApplyResponse)SendReceive(req);
            return res.CalendarItems;
        }

        private void Send(Packet packet)
        {
            using (StringWriter w = new StringWriter())
            {
                _serializer.Serialize(w, packet);
                var strData = w.ToString();
                var binData = _encoding.GetBytes(strData);
                var writeLength = (int)binData.Length;
                _writer.Write(writeLength);
                _writer.Write(binData);
                _writer.Flush();
            }
        }

        private Packet Receive()
        {
            var response = default(Packet);
            var readLength = _reader.ReadInt32();
            var binResponse = _reader.ReadBytes(readLength);
            var strResponse = _encoding.GetString(binResponse);
            using (var r = new StringReader(strResponse))
            {
                response = (Packet)_serializer.Deserialize(r);
            }
            return response;
        }

        private Packet SendReceive(Packet packet)
        {
            Send(packet);
            var res = Receive();
            return res;
        }
    }
}

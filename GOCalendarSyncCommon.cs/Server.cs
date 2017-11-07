using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace GOCalendarSyncCommon
{
    public class Server : IDisposable
    {
        public delegate Packet PacketHandler(Packet packet);

        private Encoding _encoding = Encoding.Unicode;
        private XmlSerializer _serializer = new XmlSerializer(typeof(Packet));
        private Thread _workerThread;
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private PacketHandler _handler;

        public Server(Stream input, Stream output, PacketHandler handler)
        {
            _workerThread = new Thread(RunBackground);
            _reader = new BinaryReader(input);
            _writer = new BinaryWriter(output);
            _handler = handler;
        }

        public void Start()
        {
            _workerThread.Start();
        }

        private void RunBackground()
        {
            while (true)
            {
                try
                {
                    var dataLength = _reader.ReadInt32();
                    var dataBody = _reader.ReadBytes(dataLength);
                    var strData = _encoding.GetString(dataBody);
                    var packet = default(Packet);
                    using (var packetReader = new StringReader(strData))
                    {
                        packet = (Packet)_serializer.Deserialize(packetReader);
                    }
                    var response = _handler(packet);
                    using (var packetWriter = new StringWriter())
                    {
                        _serializer.Serialize(packetWriter, response);
                        var strResponse = packetWriter.ToString();
                        var binResponse = _encoding.GetBytes(strResponse);
                        _writer.Write((Int32)binResponse.Length);
                        _writer.Write(binResponse);
                        _writer.Flush();
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        public void Dispose()
        {
            if (_workerThread.ThreadState == ThreadState.Running)
            {
                _workerThread.Interrupt();
                _workerThread.Join();
            }
        }
    }
}

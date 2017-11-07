using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GOCalendarSyncCommon
{
    public class Configuration
    {
        private static Encoding _encoding = new UTF8Encoding(false);
        private static XmlSerializer _serializer = new XmlSerializer(typeof(Configuration));

        private static string GetConfigurationFileName()
        {
            var pathAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var pathFileName = Path.Combine(pathAppData, @"FCT\GOCalendarSync\Config.xml");
            return pathFileName;
        }

        public static Configuration Load()
        {
            var self = default(Configuration);
            var pathFileName = GetConfigurationFileName();
            using (TextReader r = new StreamReader(pathFileName))
            {
                self = (Configuration)_serializer.Deserialize(r);
            }
            if (self == null)
            {
                self = new Configuration();
            }
            return self;
        }

        public string GoogleCalendarID = string.Empty;
        public string ProxyAccount = string.Empty;
        public string ProxyPassword = string.Empty;

        public void Save()
        {
            var pathFileName = GetConfigurationFileName();
            using (TextWriter w = new StreamWriter(pathFileName, false, _encoding))
            {
                _serializer.Serialize(w, this);
            }
        }
    }
}

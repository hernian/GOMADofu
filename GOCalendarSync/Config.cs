using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using GOCalendarSyncCommon;

namespace GOCalendarSync
{
    public class Config
    {
        private const string TAG = "Config";

        private static string GetConfigFilePath()
        {
            var pathAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configDir = Path.Combine(pathAppData, Constants.REL_PATH_APPDATA);
            var configPath = Path.Combine(configDir, Constants.NAME_FILE_CONFIG);
            Directory.CreateDirectory(configDir);
            return configPath;
        }

        private static Encoding GetEncoding()
        {
            var encoding = new UTF8Encoding(false);
            return encoding;
        }


        public static Config Load()
        {
            var result = new Config();
            try
            {
                var encoding = GetEncoding();
                var configPath = GetConfigFilePath();
                using (var reader = new StreamReader(configPath, encoding))
                {
                    var serializer = new XmlSerializer(typeof(Config));
                    result = (Config)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                TraceLog.TheInstance.Header(0, TAG).WriteLine(ex);
            }
            return result;
        }

        public string GoogleCalendarID = string.Empty;
        public string ProxyAccount = string.Empty;
        public bool ProxyAuthenticationEnabled = true;
        public string ProxyPassword = string.Empty;

        public void Save()
        {
            var encoding = GetEncoding();
            var configPath = GetConfigFilePath();
            using (var writer = new StreamWriter(configPath, false, encoding))
            {
                var serializer = new XmlSerializer(typeof(Config));
                serializer.Serialize(writer, this);
            }
        }
    }
}

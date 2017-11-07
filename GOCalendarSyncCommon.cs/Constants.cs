using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOCalendarSyncCommon
{
    public class Constants
    {
        public const string REL_PATH_APPDATA = @"FCT\GOMADofu";
        public const string NAME_APP = "GOMADofu";
        
        public const string NAME_FILE_CONFIG = "Config.xml";
        public const string NAME_FILE_LAST_OUTLOOK_ITEMS = "LastOutlookItems.xml";
        public const string NAME_FILE_LOG_1 = "Log1.txt";
        public const string NAME_FILE_LOG_2 = "Log2.txt";

        public const string REG_KEY_SYNC_EXE = @"Software\FCT\GOMADofu";
        public const string TAG_SYNC_EXE = "GOMADofu";
        public const int SYNC_YEAR_RANGE = 3;

    }
}

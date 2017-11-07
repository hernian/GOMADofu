using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GOCalendarSyncCommon
{
    public class TraceLog
    {
        private const string NAME_FILE_LOG_1 = Constants.REL_PATH_APPDATA + @"\Log1.txt";
        private const string NAME_FILE_LOG_2 = Constants.REL_PATH_APPDATA + @"\Log2.txt";
        private const string NAME_MUTEX_LOG = Constants.NAME_APP + "LogMutex";
        private const long SIZE_MAX_LOG = 1024L * 1024L;

        private delegate void WriteHandler(TextWriter w);

        private static TraceLog _theInstance;

        public static TraceLog TheInstance
        {
            get
            {
                if (_theInstance == null)
                {
                    _theInstance = new TraceLog();
                }
                return _theInstance;
            }
        }

        private string _pathLog1;
        private string _pathLog2;
        private FileInfo _logFileInfo;
        private Mutex _mutex;
        private Encoding _enc;

        private TraceLog()
        {
            var pathAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var pathMyAppData = Path.Combine(pathAppData, Constants.REL_PATH_APPDATA);
            _pathLog1 = Path.Combine(pathMyAppData, Constants.NAME_FILE_LOG_1);
            _pathLog2 = Path.Combine(pathMyAppData, Constants.NAME_FILE_LOG_2);
            _logFileInfo = new FileInfo(_pathLog1);
            _mutex = new Mutex(false, NAME_MUTEX_LOG);
            _enc = new UTF8Encoding(false);
        }

        public string LogFileName
        {
            get { return _pathLog1; }
        }

        public TraceLog SetLevel(int level)
        {
            return this;
        }

        public TraceLog TimeStamp()
        {
            string timeStamp = DateTime.Now.ToString("s");
            Write(timeStamp);
            return this;
        }

        public TraceLog Tag(string tag)
        {
            string tagStr = string.Format("[{0}]", tag);
            Write(tagStr);
            return this;
        }

        public TraceLog Header(int level, string tag)
        {
            SetLevel(level);
            TimeStamp();
            Tag(tag);
            return this;
        }

        public TraceLog Write(string value)
        {
            DoWrite(value);
            return this;
        }

        public TraceLog Write(object value)
        {
            return Write(value.ToString());
        }

        public TraceLog Write(string format, params object[] args)
        {
            string value = string.Format(format, args);
            return Write(value);
        }

        public TraceLog WriteLine()
        {
            DoWriteLine(string.Empty);
            return this;
        }

        public TraceLog WriteLine(string value)
        {
            DoWriteLine(value);
            return this;
        }

        public TraceLog WriteLine(object value)
        {
            return WriteLine(value.ToString());
        }

        public TraceLog WriteLine(Exception ex)
        {
            while (ex != null)
            {
                var msg = ex.ToString();
                Write(msg);
                if (msg.EndsWith("\r\n") == false)
                {
                    WriteLine();
                }
                ex = ex.InnerException;
            }
            return this;
        }

        public TraceLog WriteLine(string format, params object[] args)
        {
            string value = string.Format(format, args);
            return WriteLine(value);
        }


        private void DoWrite(string strLog)
        {
            Debug.Write(strLog);
            WriteHarness(w => w.Write(strLog));
        }

        private void DoWriteLine(string strLog)
        {
            Debug.WriteLine(strLog);
            WriteHarness(w => w.WriteLine(strLog));
        }

        private void WriteHarness(WriteHandler handler)
        {
            _mutex.WaitOne();
            try
            {
                if (File.Exists(_pathLog1))
                {
                    _logFileInfo.Refresh();
                    if (_logFileInfo.Length > SIZE_MAX_LOG)
                    {
                        ReplaceLog();
                    }
                }
                using (var w = new StreamWriter(_pathLog1, true, _enc))
                {
                    handler(w);
                }
            }
            catch (Exception /* ex */)
            {
                // ログ書き込み中に例外が投げられたが、どうしようもないので無視する
            }
            _mutex.ReleaseMutex();
        }

        private void ReplaceLog()
        {
            File.Delete(_pathLog2);
            File.Move(_pathLog1, _pathLog2);
        }
    }
}

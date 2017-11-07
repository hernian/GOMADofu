using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOCalendarSync
{
    class CommandLineOptions
    {
        private const string OPTION_HEADERS = "/-";

        private class ArgumentDescriptor
        {
            public readonly string ArgumentName;
            public readonly Action ArgumentAction;

            public ArgumentDescriptor(string name, Action action)
            {
                this.ArgumentName = name;
                this.ArgumentAction = action;
            }
        }

        public enum ActionTypes
        {
            Regist,
            Config,
            Sync
        }

        public CommandLineOptions(string[] args)
        {
            this.Debug = GetDebugSetting();
            this.ActionType = ActionTypes.Config;
            var argDescs = CreateArgumentDescriptors();
            foreach (var arg in args)
            {
                if (arg.Length == 0)
                {
                    continue;
                }
                var firstChar = arg[0];
                if (OPTION_HEADERS.Contains(firstChar) == false)
                {
                    continue;
                }
                var option = arg.Substring(1);
                var argDesc = argDescs.Find(ad => string.Compare(ad.ArgumentName, option, true) == 0);
                if (argDesc != default(ArgumentDescriptor))
                {
                    argDesc.ArgumentAction();
                }
            }
        }

        private List<ArgumentDescriptor> CreateArgumentDescriptors()
        {
            var list = new List<ArgumentDescriptor>();
            list.Add(new ArgumentDescriptor("debug", () => this.Debug = true));
            list.Add(new ArgumentDescriptor("regist", () => this.ActionType = ActionTypes.Regist));
            list.Add(new ArgumentDescriptor("config", () => this.ActionType = ActionTypes.Config));
            list.Add(new ArgumentDescriptor("sync", () => this.ActionType = ActionTypes.Sync));
            return list;
        }

        public bool Debug
        {
            private set;
            get;
        }

        public ActionTypes ActionType
        {
            private set;
            get;
        }

        private bool GetDebugSetting()
        {
            bool result = false;
            try
            {
                var debugValue = (int)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\FCT\GOCalendarSync", "debug", 0);
                result = (debugValue != 0);
            }
            catch (Exception)
            {
            }
            return result;
        }
    }
}

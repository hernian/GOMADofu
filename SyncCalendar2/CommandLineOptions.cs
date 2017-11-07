using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncCalendar
{
    class CommandLineOptions
    {
        private const string OPTION_HEADER = "/-";

        private delegate void ArgumentAction(CommandLineOptions self);
        private static Dictionary<string, ArgumentAction> ACTION_DICT = new Dictionary<string, ArgumentAction>();

        public enum ActionTypes
        {
            Regist,
            Config,
            Sync
        }

        static CommandLineOptions()
        {
            ACTION_DICT.Add("debug", self => { self.Debug = true; });
            ACTION_DICT.Add("regist", self => { self.Action = ActionTypes.Regist; });
            ACTION_DICT.Add("config", self => { self.Action = ActionTypes.Config; });
            ACTION_DICT.Add("sync", self => { self.Action = ActionTypes.Sync; });
        }

        public CommandLineOptions(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.Length == 0)
                {
                    continue;
                }
                var firstChar = arg[0];
                if (OPTION_HEADER.Contains(firstChar) == false)
                {
                    continue;
                }
                var option = arg.Substring(1);
                var action = default(ArgumentAction);
                if (ACTION_DICT.TryGetValue(option, out action))
                {
                    action(this);
                }
            }
        }

        public bool Debug
        {
            private set;
            get;
        }

        public ActionTypes Action
        {
            private set;
            get;
        }
    }
}

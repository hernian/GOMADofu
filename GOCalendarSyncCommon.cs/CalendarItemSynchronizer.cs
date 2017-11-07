using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOCalendarSyncCommon
{
    public class CalendarItemSynchronizer
    {
        private const string TAG = "同期";
        private Dictionary<Guid, GatheredCalenderItems> _gatheredCalendarItemDict = new Dictionary<Guid, GatheredCalenderItems>();
        private CalendarItemCollection _googleCalendarItems;
        private CalendarItemCollection _outlookCalendarItems;

        public CalendarItemSynchronizer(CalendarItemCollection googleCalendarItems, CalendarItemCollection outlookCalendarItems)
        {
            _googleCalendarItems = googleCalendarItems;
            _outlookCalendarItems = outlookCalendarItems;
        }

        public void Synchronize()
        {
            // 1st. GUIDで一致するものを集める
            AddCalendarItemsWithGuid(_googleCalendarItems, GatheredCalenderItems.CalendarItemTypes.Google);
            AddCalendarItemsWithGuid(_outlookCalendarItems, GatheredCalenderItems.CalendarItemTypes.Outlook);

            // 2nd. 名前、場所、開始・終了時刻で一致するものを集める
            AddCalendarItemsWithoutGuid(_googleCalendarItems, GatheredCalenderItems.CalendarItemTypes.Google);
            AddCalendarItemsWithoutGuid(_outlookCalendarItems, GatheredCalenderItems.CalendarItemTypes.Outlook);

            TraceLog.TheInstance.Header(0, TAG).WriteLine("同期イベント");

            // 集めたものを同期する
            foreach (var gatheredCalendarItems in _gatheredCalendarItemDict.Values)
            {
                var googleItemCount = CountItems(gatheredCalendarItems.GoogleItems, i => i.Cancelled == false);
                var outlookItemCount = CountItems(gatheredCalendarItems.OutlookItems, i => i.Cancelled == false);

                var r = IsChangeRequired(gatheredCalendarItems, googleItemCount, outlookItemCount);
                if (r)
                {
                    LogGatheredCalendarItems(gatheredCalendarItems);
                }
                SynchronizeGatheredCalendarItems(gatheredCalendarItems, googleItemCount, outlookItemCount);
            }
#if false
            var fileName = @"d:\temp\sync.txt";
            var encodnig = new UTF8Encoding(false);
            using (var w = new System.IO.StreamWriter(fileName, false, encodnig))
            {
                foreach (var g in _gatheredCalendarItemDict.Values)
                {
                    w.WriteLine(string.Format("guid:{0} ****", g.Guid));
                    OutputCalendarItems(w, "google", g.GoogleItems);
                    OutputCalendarItems(w, "outlook", g.OutlookItems);
                    w.WriteLine();
                }
            }
#endif
        }

        private void OutputCalendarItems(System.IO.TextWriter w, string title, IEnumerable<CalendarItem> items)
        {
            w.WriteLine(title);
            foreach (var i in items)
            {
                var startStr = i.Start.ToString("g");
                var endStr = i.End.ToString("g");
                var str = string.Format("  name:{0} start:{1}, end:{2}, location:{3}", i.Name, startStr, endStr, i.Location);
                w.WriteLine(str);
            }
        }

        private void AddCalendarItemsWithGuid(CalendarItemCollection calendarItems, GatheredCalenderItems.CalendarItemTypes itemType)
        {
            foreach (var calendarItem in calendarItems)
            {
                if (calendarItem.SyngronizeGuid != Guid.Empty)
                {
                    var guid = calendarItem.SyngronizeGuid;
                    var gatheredCalendarItems = default(GatheredCalenderItems);
                    var r = _gatheredCalendarItemDict.TryGetValue(guid, out gatheredCalendarItems);
                    if (r == false)
                    {
                        gatheredCalendarItems = new GatheredCalenderItems(guid);
                        _gatheredCalendarItemDict[guid] = gatheredCalendarItems;
                    }
                    gatheredCalendarItems.AddItem(calendarItem, itemType);
                }
            }
        }

        private void AddCalendarItemsWithoutGuid(CalendarItemCollection calendarItems, GatheredCalenderItems.CalendarItemTypes itemType)
        {
            foreach (var calendarItem in calendarItems)
            {
                if (calendarItem.SyngronizeGuid == Guid.Empty)
                {
                    var gatheredCalendarItems = FindGatheredCalendarItems(calendarItem);
                    if (gatheredCalendarItems == null)
                    {
                        var guid = Guid.NewGuid();
                        gatheredCalendarItems = new GatheredCalenderItems(guid);
                        _gatheredCalendarItemDict[guid] = gatheredCalendarItems;
                    }
                    gatheredCalendarItems.AddItem(calendarItem, itemType);
                }
            }
        }

        private void LogGatheredCalendarItems(GatheredCalenderItems gatheredCalendarItems)
        {
            var log = TraceLog.TheInstance;
            var guid = gatheredCalendarItems.Guid;
            log.WriteLine("  同期イベント集合 GUID:{0}", guid.ToString("B"));
            foreach (var calendarItem in gatheredCalendarItems.GoogleItems)
            {
                log.WriteLine("    [Google ] 名前:{0} 開始時刻:{1} 終了時刻:{2} GUID:{3} キャンセル:{4,-5} 場所:{5}",
                                    calendarItem.Name,
                                    calendarItem.Start.ToString("u"),
                                    calendarItem.End.ToString("u"),
                                    calendarItem.SyngronizeGuid.ToString("B"),
                                    calendarItem.Cancelled,
                                    calendarItem.Location);
            }
            foreach (var calendarItem in gatheredCalendarItems.OutlookItems)
            {
                log.WriteLine("    [Outlook] 名前:{0} 開始時刻:{1} 終了時刻:{2} GUID:{3} キャンセル:{4,-5} 場所:{5}",
                                    calendarItem.Name,
                                    calendarItem.Start.ToString("u"),
                                    calendarItem.End.ToString("u"),
                                    calendarItem.SyngronizeGuid.ToString("B"),
                                    calendarItem.Cancelled,
                                    calendarItem.Location);
            }
        }

        private bool IsChangeRequired(GatheredCalenderItems gatheredCalendarItems, int googleItemCount, int outlookItemCount)
        {
            var guid = gatheredCalendarItems.Guid;
            var key = gatheredCalendarItems.Key;

            if (key.Cancelled == false)
            {
                if ((googleItemCount == 0) || (outlookItemCount == 0))
                {
                    return true;
                }
            }
            foreach (var calendarItem in gatheredCalendarItems.Items)
            {
                var r = IsChangeRequired(guid, key, calendarItem);
                if (r)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsChangeRequired(Guid guid, CalendarItem key, CalendarItem calendarItem)
        {
            if (calendarItem.SyngronizeGuid != guid)
            {
                return true;
            }
            if (calendarItem.Name != key.Name)
            {
                return true;
            }
            if (calendarItem.Start != key.Start)
            {
                return true;
            }
            if (calendarItem.End != key.End)
            {
                return true;
            }
            if (calendarItem.AllDayEvent != key.AllDayEvent)
            {
                return true;
            }
            if (calendarItem.Cancelled != key.Cancelled)
            {
                return true;
            }
            if (calendarItem.Location != key.Location)
            {
                return true;
            }
            if (calendarItem.Body != key.Body)
            {
                return true;
            }
            return false;
        }

        private void SynchronizeGatheredCalendarItems(GatheredCalenderItems gatheredCalendarItems, int googleItemCount, int outlookItemCount)
        {
            var guid = gatheredCalendarItems.Guid;
            var key = gatheredCalendarItems.Key;

            if (key.Cancelled == false)
            {
                // 予定の同期は、非キャンセルな項目に全てに対して行う。
                // ただし、非キャンセルな項目が無ければ新たに追加する。
                if (googleItemCount == 0)
                {
                    var newCalendarItem = new CalendarItem();
                    _googleCalendarItems.Add(newCalendarItem);
                    gatheredCalendarItems.AddItem(newCalendarItem, GatheredCalenderItems.CalendarItemTypes.Google);
                }
                if (outlookItemCount == 0)
                {
                    var newCalendarItem = new CalendarItem();
                    _outlookCalendarItems.Add(newCalendarItem);
                    gatheredCalendarItems.AddItem(newCalendarItem, GatheredCalenderItems.CalendarItemTypes.Outlook);
                }
                foreach (var calendarItem in gatheredCalendarItems.Items)
                {
                    if (calendarItem.Cancelled == false)
                    {
                        SynchronizeItem(guid, key, calendarItem);
                    }
                }
            }
            else
            {
                // 予定のキャンセルは、マッチした全項目に対して行う。
                foreach (var calendarItem in gatheredCalendarItems.Items)
                {
                    if (calendarItem.Cancelled == false)
                    {
                        calendarItem.Cancelled = true;
                        calendarItem.Changed = true;
                    }
                }
            }
        }

        private void SynchronizeItem(Guid guid, CalendarItem key, CalendarItem calendarItem)
        {
            var changed = false;
            if (calendarItem.SyngronizeGuid != guid)
            {
                calendarItem.SyngronizeGuid = guid;
                changed = true;
            }
            if (calendarItem.Name != key.Name)
            {
                calendarItem.Name = key.Name;
                changed = true;
            }
            if (calendarItem.Start != key.Start)
            {
                calendarItem.Start = key.Start;
                changed = true;
            }
            if (calendarItem.End != key.End)
            {
                calendarItem.End = key.End;
                changed = true;
            }
            if (calendarItem.AllDayEvent != key.AllDayEvent)
            {
                calendarItem.AllDayEvent = key.AllDayEvent;
                changed = true;
            }
            if (calendarItem.Cancelled != key.Cancelled)
            {
                calendarItem.Cancelled = key.Cancelled;
                changed = true;
            }
            if (calendarItem.Location != key.Location)
            {
                calendarItem.Location = key.Location;
                changed = true;
            }
            if (calendarItem.Body != key.Body)
            {
                calendarItem.Body = key.Body;
                changed = true;
            }
            calendarItem.Changed = changed;
        }

        private CalendarItem CreateCalendarItem(Guid guid, CalendarItem key)
        {
            var calendarItem = new CalendarItem();
            calendarItem.SyngronizeGuid = guid;
            calendarItem.Name = key.Name;
            calendarItem.Start = key.Start;
            calendarItem.End = key.End;
            calendarItem.AllDayEvent = key.AllDayEvent;
            calendarItem.Cancelled = key.Cancelled;
            calendarItem.Location = key.Location;
            calendarItem.Body = key.Body;
            calendarItem.Changed = true;
            return calendarItem;
        }

        private GatheredCalenderItems FindGatheredCalendarItems(CalendarItem calendarItem)
        {
            foreach (var kv in _gatheredCalendarItemDict)
            {
                var m = MatchCalendarItems(kv.Value.Key, calendarItem);
                if (m)
                {
                    return kv.Value;
                }
            }
            return null;
        }

        private bool MatchCalendarItems(CalendarItem lhs, CalendarItem rhs)
        {
            if (lhs.Name != rhs.Name)
            {
                return false;
            }
            if (lhs.AllDayEvent != rhs.AllDayEvent)
            {
                return false;
            }
            if (lhs.Start != rhs.Start)
            {
                return false;
            }
            if (lhs.End != rhs.End)
            {
                return false;
            }
            return true;
        }

        private int CountItems(IEnumerable<CalendarItem> items, Predicate<CalendarItem> filter)
        {
            var count = default(int);
            foreach (var item in items)
            {
                if (filter(item))
                {
                    ++count;
                }
            }
            return count;
        }

    }
}

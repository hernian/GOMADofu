using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using GOCalendarSyncCommon;

namespace SyncCalendar
{
    class GoogleCalendarProvider
    {
        private const string STORE_NAME = @"FCT\GOCalendarSync";
        private const string APP_NAME = "GOCalendarSync";
        private const string EX_PROPERTY_NAME_SYNC_GUID = "GOCalendarSyncGUID";
        private const string TIME_FORMAT_ALL_DAY = "yyyy-MM-dd";
        private const string EVENT_STATUS_CONFIRMED = "confirmed";
        private const string EVENT_STATUS_CANCELLED = "cancelled";
        private const string TAG_LOG = "Google";

        private static string[] SCOPES = { CalendarService.Scope.Calendar };
        private static ClientSecrets CLIENT_SECRETS = new ClientSecrets
        {
            ClientId = "594486482957-hf909unhprtm475l3qcdmvcn1dqkih79.apps.googleusercontent.com",
            ClientSecret = "dtqjtJ_fKgfiKID8-X19KDPb"
        };

        private string _calendarID;
        private CalendarService _service = null;
        private EventDictionary _eventDict = new EventDictionary();

        public GoogleCalendarProvider(string calendarID)
        {
            _calendarID = calendarID;

            var fileDataStore = new FileDataStore(STORE_NAME);
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                CLIENT_SECRETS, SCOPES, "user", CancellationToken.None, fileDataStore
                ).Result;
            var initializer = new BaseClientService.Initializer();
            initializer.HttpClientInitializer = credential;
            initializer.ApplicationName = APP_NAME;
            _service = new CalendarService(initializer);
        }

        public void EndSync()
        {
            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
        }

        public CalendarItemCollection GetRange(DateTime start, DateTime end)
        {
            var calendarItems = new CalendarItemCollection();
            var req = _service.Events.List(_calendarID);
            req.SingleEvents = false;
            req.ShowDeleted = true;
            req.TimeMin = start;
            req.TimeMax = end;
            string pageToken = null;
            do
            {
                req.PageToken = pageToken;
                var events = req.Execute();
                foreach (Event ev in events.Items)
                {
                    if (ev.Recurrence != null)
                    {
                        // 繰り返しイベントは同期の対象外
                        continue;
                    }
                    var sd = GetDateTime(ev.Start);
                    var ed = GetDateTime(ev.End);
                    if ((ed <= start) || (end <= start))
                    {
                        // 範囲外は無視する
                        continue;
                    }
                    _eventDict.Add(ev);
                    CalendarItem calendarItem = CreateCalendarItem(ev);
                    calendarItems.Add(calendarItem);
                }
                events.NextPageToken = pageToken;
                pageToken = events.NextPageToken;
            }
            while (pageToken != null);
            return calendarItems;
        }

        public void Apply(CalendarItemCollection calendarItems)
        {
            Log(0, "更新開始");
            foreach (var calendarItem in calendarItems)
            {
                try
                {
                    if (calendarItem.Changed)
                    {
                        if (calendarItem.ID == string.Empty)
                        {
                            CreateItem(calendarItem);
                        }
                        else if (calendarItem.Cancelled)
                        {
                            DeleteItem(calendarItem);
                        }
                        else
                        {
                            UpdateItem(calendarItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log(0, ex);
                }
            }
            Log(0, "更新終了");
        }

        private void UpdateItem(CalendarItem calendarItem)
        {
            var ev = _eventDict.Find(calendarItem.ID);
            if (ev == null)
            {
                // あるはずのEventオブジェクトが見つからないから無視する
                return;
            }
            var orgCalendarItem = CreateCalendarItem(ev);
            Log(0, "変更前", orgCalendarItem);
            Log(0, "変更後", calendarItem);
            UpdateEvent(ev, calendarItem);
            var req = _service.Events.Update(ev, _calendarID, ev.Id);
            req.Execute();
        }

        private void CreateItem(CalendarItem calendarItem)
        {
            Log(0, "新規", calendarItem);
            var ev = new Event();
            UpdateEvent(ev, calendarItem);
            var req = _service.Events.Insert(ev, _calendarID);
            req.Execute();
        }

        private void DeleteItem(CalendarItem calendarItem)
        {
            var ev = _eventDict.Find(calendarItem.ID);
            if (ev == null)
            {
                // あるはずのEventオブジェクトが見つからないから無視する
                return;
            }

            Log(0, "削除", calendarItem);
            var req = _service.Events.Delete(_calendarID, ev.Id);
            req.Execute();
        }

        private void UpdateEvent(Event ev, CalendarItem calendarItem)
        {
            ev.Summary = calendarItem.Name;
            ev.Location = calendarItem.Location;
            ev.Start = GetEventDateTime(calendarItem.Start, calendarItem.AllDayEvent);
            ev.End = GetEventDateTime(calendarItem.End, calendarItem.AllDayEvent);
            if (ev.Status != EVENT_STATUS_CONFIRMED)
            {
                ev.Status = EVENT_STATUS_CONFIRMED;
            }
            ev.Description = calendarItem.Body;
            if (ev.ExtendedProperties == null)
            {
                ev.ExtendedProperties = new Event.ExtendedPropertiesData();
            }
            if (ev.ExtendedProperties.Private == null)
            {
                ev.ExtendedProperties.Private = new Dictionary<string, string>();
            }
            ev.ExtendedProperties.Private[EX_PROPERTY_NAME_SYNC_GUID] = calendarItem.SyngronizeGuid.ToString();
        }

        private EventDateTime GetEventDateTime(DateTime dateTime, bool allday)
        {
            var result = new EventDateTime();
            if (allday)
            {
                result.Date = dateTime.ToString(TIME_FORMAT_ALL_DAY);
            }
            else
            {
                result.DateTime = dateTime;
            }
            return result;
        }

        private CalendarItem CreateCalendarItem(Event ev)
        {
            var calendarItem = new CalendarItem();
            calendarItem.ID = ev.Id;
            calendarItem.SyngronizeGuid = GetSynchronizedGuid(ev);
            calendarItem.Name = (ev.Summary != null) ? ev.Summary : string.Empty;
            var a = ev.Start;
            calendarItem.Start = GetDateTime(ev.Start);
            calendarItem.End = GetDateTime(ev.End);
            calendarItem.LastModified = ev.Updated.HasValue ? ev.Updated.Value : new DateTime(0L);
            calendarItem.AllDayEvent = (ev.Start.DateTime.HasValue == false);
            calendarItem.Cancelled = (ev.Status == EVENT_STATUS_CANCELLED);
            calendarItem.Location = (ev.Location != null) ? ev.Location : string.Empty;
            calendarItem.Body = (ev.Description != null) ? ev.Description : string.Empty;
            return calendarItem;
        }

        private DateTime GetDateTime(EventDateTime evDateTime)
        {
            return evDateTime.DateTime.HasValue ? evDateTime.DateTime.Value : DateTime.ParseExact(evDateTime.Date, "yyyy-MM-dd", null);
        }

        private Guid GetSynchronizedGuid(Event ev)
        {
            if (ev.ExtendedProperties == null)
            {
                return Guid.Empty;
            }
            if (ev.ExtendedProperties.Private == null)
            {
                return Guid.Empty;
            }
            var strGuid = default(string);
            if (ev.ExtendedProperties.Private.TryGetValue(EX_PROPERTY_NAME_SYNC_GUID, out strGuid) == false)
            {
                return Guid.Empty;
            }
            var guid = default(Guid);
            if (Guid.TryParse(strGuid, out guid) == false)
            {
                return Guid.Empty;
            }
            return guid;
        }

        private void Log(int level, object obj)
        {
            Log(level, obj.ToString());
        }

        private void Log(int level, string logStr)
        {
            TraceLog.TheInstance.SetLevel(level).TimeStamp().Tag(TAG_LOG).WriteLine(logStr);
        }

        private void Log(int level, string format, params object[] args)
        {
            string logStr = string.Format(format, args);
            Log(level, logStr);
        }

        private void Log(int level, string action, CalendarItem calendarItem)
        {
            string logStr = string.Format("{0} 名前:{1} 開始時刻:{2} 終了時刻:{3} 場所:{4} 本文:{5}",
                                action,
                                calendarItem.Name,
                                calendarItem.Start,
                                calendarItem.End,
                                calendarItem.Location,
                                calendarItem.Body);
            Log(level, logStr);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOCalendarSyncCommon
{
    public class CalendarItemCollection : IEnumerable<CalendarItem>
    {
        /// <summary>
        /// 全要素を列挙するためのリスト
        /// </summary>
        private List<CalendarItem> _items = new List<CalendarItem>();

        public CalendarItemCollection()
        {
        }

        public void Add(CalendarItem item)
        {
            _items.Add(item);
        }

        #region IEnumerable<CalendarItem> メンバー

        public IEnumerator<CalendarItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        #endregion

        #region IEnumerable メンバー

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        #endregion


    }
}

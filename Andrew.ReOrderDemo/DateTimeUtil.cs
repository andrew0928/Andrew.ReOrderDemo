using System;
using System.Collections.Generic;
using System.Text;

namespace Andrew.ReOrderDemo
{

    // 請參考介紹文章: https://columns.chicken-house.net/2022/05/29/datetime-mock/
    public class DateTimeUtil
    {
        private static DateTimeUtil _instance = null;

        public static DateTimeUtil Instance => _instance;
        public static void Init(DateTime expectedTimeNow)
        {
            if (_instance != null) throw new InvalidOperationException("DateTimeUtil was initialized. Call Reset() before re-init.");
            _instance = new DateTimeUtil(expectedTimeNow);
        }
        public static void Reset()
        {
            _instance = null;
        }

        /// <summary>
        /// 時間跨過每天的 00:00:00 時，會觸發 OnDayPass 事件
        /// 已知問題: 若在真實的時間軸 (例如執行 long running job, 或是 sleep) 進行度過跨日線的話, 不會立即觸發該日的事件。
        /// 精確觸發的時間點只有這兩個: 經過 .TimePass() 來移動時間軸，或是透過 DateTimeUtil.Instance.Now 存取目前時間。
        /// </summary>
        public event EventHandler<TimePassEventArgs> RaiseSecondPassEvent;

        public class TimePassEventArgs : EventArgs
        {
            public DateTime OccurTime;
        }

        /// <summary>
        /// 封裝過的時間軸，與實際的時間軸的時間差
        /// </summary>
        private TimeSpan _realtime_offset = TimeSpan.Zero;

        private DateTime _last_check_event_time = DateTime.MinValue;

        private DateTimeUtil(DateTime expectedTime)
        {
            this._realtime_offset = expectedTime - DateTime.Now;
            this._last_check_event_time = expectedTime;

            this.RaiseSecondPassEvent += (sender, args) => { Console.WriteLine($"- event: RaiseSecondPassEvent({args.OccurTime}, {this.Now})"); };
        }

        public DateTime Now
        {
            get
            {
                var result = DateTime.Now.Add(this._realtime_offset);
                this.Seek_LastEventCheckTime(result);
                return result;
            }
        }

        private void Seek_LastEventCheckTime(DateTime checkTime)
        {
            while (this._last_check_event_time < checkTime)
            {
                // 精確到秒
                DateTime next_check_event_time = new DateTime(
                    this._last_check_event_time.Year,
                    this._last_check_event_time.Month,
                    this._last_check_event_time.Day,
                    this._last_check_event_time.Hour,
                    this._last_check_event_time.Minute,
                    this._last_check_event_time.Second).AddSeconds(1.0);
                if (next_check_event_time > checkTime) break;


                this._last_check_event_time = next_check_event_time;
                this.RaiseSecondPassEvent?.Invoke(this, new TimePassEventArgs()
                {
                    OccurTime = this._last_check_event_time
                });
            }
            this._last_check_event_time = checkTime;
        }

        public void TimePass(TimeSpan duration)
        {
            if (duration > TimeSpan.Zero)
            {
                // normal case
                this._realtime_offset += duration;
                this.Seek_LastEventCheckTime(this.Now);
            }
            else if (duration.TotalMilliseconds > -10.0)
            {
                // noise, ignore
            }
            else
            {
                throw new ArgumentOutOfRangeException($"duration is less then zero. ({duration.TotalMilliseconds})");
            }
        }

        public void TimeSeek(DateTime seekTimeTo)
        {
            this.TimePass(seekTimeTo - this.Now);
        }
    }

}

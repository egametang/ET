using System;

namespace ET
{
    public class TimeInfo: IDisposable
    {
        public static TimeInfo Instance = new TimeInfo();

        private int timeZone;
        
        public int TimeZone
        {
            get
            {
                return this.timeZone;
            }
            set
            {
                this.timeZone = value;
                dt = dt1970.AddHours(TimeZone);
            }
        }
        
        private readonly DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        public long ServerMinusClientTime { private get; set; }

        public long FrameTime;

        private TimeInfo()
        {
            this.FrameTime = this.ClientNow();
        }

        public void Update()
        {
            this.FrameTime = this.ClientNow();
        }
        
        /// <summary> 
        /// 根据时间戳获取时间 
        /// </summary>  
        public DateTime ToDateTime(long timeStamp)
        {
            return dt.AddTicks(timeStamp * 10000);
        }
        
        // 线程安全
        public long ClientNow()
        {
            return (DateTime.Now.Ticks - this.dt1970.Ticks) / 10000;
        }
        
        public long ServerNow()
        {
            return ClientNow() + Instance.ServerMinusClientTime;
        }
        
        public long ClientFrameTime()
        {
            return this.FrameTime;
        }
        
        public long ServerFrameTime()
        {
            return this.FrameTime + Instance.ServerMinusClientTime;
        }
        
        public long Transition(DateTime d)
        {
            return (d.Ticks - dt.Ticks) / 10000;
        }

        public void Dispose()
        {
            Instance = null;
        }
    }
}
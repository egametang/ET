using System;

namespace ET
{
    /// <summary>
    /// 时间信息类
    /// </summary>
    public class TimeInfo: Singleton<TimeInfo>, ISingletonUpdate
    {
        /// <summary>
        /// 时区
        /// </summary>
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

        /// <summary>
        /// 表示1970年1月1日0点0分0秒的UTC时间
        /// </summary>
        private DateTime dt1970;

        /// <summary>
        /// 表示1970年1月1日0点0分0秒加上时区的时间
        /// </summary>
        private DateTime dt;

        /// <summary>
        /// 表示服务器时间和客户端时间的差值，只能在类内部获取，在类外部设置
        /// </summary>
        public long ServerMinusClientTime { private get; set; }

        /// <summary>
        /// 帧时间
        /// </summary>
        public long FrameTime;

        public TimeInfo()
        {
            // 初始化时间
            this.dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            this.dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // 初始化FrameTime的值，调用ClientNow方法获取当前客户端时间
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

        /// <summary>
        /// 线程安全
        /// 获取当前客户端时间
        /// </summary>
        /// <returns></returns>
        public long ClientNow()
        {
            return (DateTime.UtcNow.Ticks - this.dt1970.Ticks) / 10000;
        }

        /// <summary>
        /// 获取当前服务器时间
        /// </summary>
        /// <returns></returns>
        public long ServerNow()
        {
            return ClientNow() + Instance.ServerMinusClientTime;
        }

        /// <summary>
        /// 获取当前客户端帧时间
        /// </summary>
        /// <returns></returns>
        public long ClientFrameTime()
        {
            return this.FrameTime;
        }

        /// <summary>
        /// 获取当前服务器帧时间
        /// </summary>
        /// <returns></returns>
        public long ServerFrameTime()
        {
            return this.FrameTime + Instance.ServerMinusClientTime;
        }

        /// <summary>
        /// 将一个DateTime对象转换为对应的时间戳
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public long Transition(DateTime d)
        {
            return (d.Ticks - dt.Ticks) / 10000;
        }
    }
}
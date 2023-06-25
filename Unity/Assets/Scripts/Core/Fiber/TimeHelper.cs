using System;

namespace ET
{
    public static class TimeHelper
    {
        public const long OneDay = 86400000;
        public const long Hour = 3600000;
        public const long Minute = 60000;
        
        /// <summary>
        /// 客户端时间
        /// </summary>
        /// <returns></returns>
        public static long ClientNow()
        {
            return Fiber.Instance.TimeInfo.ClientNow();
        }

        public static long ClientNowSeconds()
        {
            return ClientNow() / 1000;
        }

        public static DateTime DateTimeNow()
        {
            return DateTime.Now;
        }

        public static long ServerNow()
        {
            return Fiber.Instance.TimeInfo.ServerNow();
        }

        public static long ClientFrameTime()
        {
            return Fiber.Instance.TimeInfo.ClientFrameTime();
        }
        
        public static long ServerFrameTime()
        {
            return Fiber.Instance.TimeInfo.ServerFrameTime();
        }
    }
}
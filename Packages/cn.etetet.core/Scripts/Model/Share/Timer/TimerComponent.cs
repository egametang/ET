using System;
using System.Collections.Generic;

namespace ET
{
    public enum TimerClass
    {
        None,
        OnceTimer,
        OnceWaitTimer,
        RepeatedTimer,
    }

    public struct TimerAction
    {
        public TimerAction(TimerClass timerClass, long startTime, long time, int type, object obj)
        {
            this.TimerClass = timerClass;
            this.StartTime = startTime;
            this.Object = obj;
            this.Time = time;
            this.Type = type;
        }
        
        public TimerClass TimerClass;
        
        public int Type;

        public object Object;

        public long StartTime;

        public long Time;
    }

    public struct TimerCallback
    {
        public object Args;
    }

    [ComponentOf(typeof(Scene))]
    public class TimerComponent: Entity, IAwake, IUpdate
    {
        /// <summary>
        /// key: time, value: timer id
        /// </summary>
        public readonly MultiMap<long, long> timeId = new();

        public readonly Queue<long> timeOutTime = new();

        public readonly Queue<long> timeOutTimerIds = new();

        public readonly Dictionary<long, TimerAction> timerActions = new();

        public long idGenerator;

        // 记录最小时间，不用每次都去MultiMap取第一个值
        public long minTime = long.MaxValue;
    }
}
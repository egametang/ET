using System.Collections.Generic;
using Common.Base;
using Common.Event;
using Common.Helper;
using MongoDB.Bson;

namespace Model
{
    public class TimerComponent: Component<World>
    {
        private class Timer
        {
            public ObjectId Id { get; set; }
            public long Time { get; set; }
            public int CallbackId { get; set; }
            public Env Env { get; set; }
        }

        private readonly Dictionary<ObjectId, Timer> timers = new Dictionary<ObjectId, Timer>();

        /// <summary>
        /// key: time, value: timer id
        /// </summary>
        private readonly MultiMap<long, ObjectId> timeId = new MultiMap<long, ObjectId>();

        public ObjectId Add(long time, int callbackId, Env env)
        {
            Timer timer = new Timer
            {
                Id = ObjectId.GenerateNewId(), 
                Time = time, 
                CallbackId = callbackId,
                Env = env
            };
            this.timers[timer.Id] = timer;
            this.timeId.Add(timer.Time, timer.Id);
            return timer.Id;
        }

        public void Remove(ObjectId id)
        {
            Timer timer;
            if (!this.timers.TryGetValue(id, out timer))
            {
                return;
            }
            this.timeId.Remove(timer.Time, timer.Id);
        }

        public void Update()
        {
            long timeNow = TimeHelper.Now();
            List<long> timeoutTimer = new List<long>();
            foreach (long time in this.timeId.Keys)
            {
                if (time > timeNow)
                {
                    break;
                }
                timeoutTimer.Add(time);
            }

            foreach (long key in timeoutTimer)
            {
                List<ObjectId> timeOutId = this.timeId[key];
                foreach (ObjectId id in timeOutId)
                {
                    Timer timer;
                    if (!this.timers.TryGetValue(id, out timer))
                    {
                        continue;
                    }
                    this.Remove(id);
                    World.Instance.GetComponent<EventComponent<CallbackAttribute>>().Run(timer.CallbackId, timer.Env);
                }
            }
        }
    }
}

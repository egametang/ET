using System.Collections.Generic;

namespace ETModel
{
    public class CG_GateSessionKeyComponent : Component
    {
        private readonly Dictionary<long, long> sessionKey = new Dictionary<long, long>();

        public void Add(long key, long userId)
        {
            this.sessionKey.Add(key, userId);
            this.TimeoutRemoveKey(key);
        }

        public long Get(long key)
        {
            long userId;
            this.sessionKey.TryGetValue(key, out userId);
            return userId;
        }

        public void Remove(long key)
        {
            this.sessionKey.Remove(key);
        }

        private async void TimeoutRemoveKey(long key)
        {
            await Game.Scene.GetComponent<TimerComponent>().WaitAsync(20000); //20秒有效时间
            this.sessionKey.Remove(key);
        }
    }
}

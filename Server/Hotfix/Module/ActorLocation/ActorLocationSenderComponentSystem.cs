using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class ActorLocationSenderComponentSystem : StartSystem<ActorLocationSenderComponent>
    {
        public override void Start(ActorLocationSenderComponent self)
        {
            StartAsync(self).NoAwait();
        }
        
        // 每10s扫描一次过期的actorproxy进行回收,过期时间是1分钟
        public async ETVoid StartAsync(ActorLocationSenderComponent self)
        {
            List<long> timeoutActorProxyIds = new List<long>();

            while (true)
            {
                await Game.Scene.GetComponent<TimerComponent>().WaitAsync(10000);

                if (self.IsDisposed)
                {
                    return;
                }

                timeoutActorProxyIds.Clear();

                long timeNow = TimeHelper.Now();
                foreach (long id in self.ActorLocationSenders.Keys)
                {
                    ActorLocationSender actorLocationMessageSender = self.ActorLocationSenders[id];
                    if (actorLocationMessageSender == null)
                    {
                        continue;
                    }

                    if (timeNow < actorLocationMessageSender.LastSendTime + 60 * 1000)
                    {
                        continue;
                    }

                    timeoutActorProxyIds.Add(id);
                }

                foreach (long id in timeoutActorProxyIds)
                {
                    self.Remove(id);
                }
            }
        }
    }
}
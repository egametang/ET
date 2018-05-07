using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class ActorMessageSenderComponentSystem : StartSystem<ActorMessageSenderComponent>
    {
        // 每10s扫描一次过期的actorproxy进行回收,过期时间是1分钟
        public override async void Start(ActorMessageSenderComponent self)
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
                foreach (long id in self.ActorMessageSenders.Keys)
                {
                    ActorMessageSender actorMessageSender = self.Get(id);
                    if (actorMessageSender == null)
                    {
                        continue;
                    }

                    if (timeNow < actorMessageSender.LastSendTime + 60 * 1000)
                    {
                        continue;
                    }

                    actorMessageSender.Error = ErrorCode.ERR_ActorTimeOut;
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
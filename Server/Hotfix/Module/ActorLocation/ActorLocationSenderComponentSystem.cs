using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class ActorLocationSenderComponentSystem : StartSystem<ActorLocationSenderComponent>
    {
        public override void Start(ActorLocationSenderComponent self)
        {
            StartAsync(self).Coroutine();
        }
        
        // 每10s扫描一次过期的actorproxy进行回收,过期时间是1分钟
        // 可能由于bug或者进程挂掉，导致ActorLocationSender发送的消息没有确认，结果无法自动删除，每一分钟清理一次这种ActorLocationSender
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

                    if (timeNow < actorLocationMessageSender.LastRecvTime + 60 * 1000)
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

    public static class ActorLocationSenderComponent_Ex
    {
        public static async ETTask<ActorLocationSender> Get(this ActorLocationSenderComponent self, long id)
        {
            if (id == 0)
            {
                throw new Exception($"actor id is 0");
            }
            if (self.ActorLocationSenders.TryGetValue(id, out ActorLocationSender actorLocationSender))
            {
                return actorLocationSender;
            }

            actorLocationSender = ComponentFactory.CreateWithId<ActorLocationSender>(id);
            actorLocationSender.Parent = self;
            await actorLocationSender.GetActorId();
            self.ActorLocationSenders[id] = actorLocationSender;
            return actorLocationSender;
        }

        public static void Remove(this ActorLocationSenderComponent self, long id)
        {
            if (!self.ActorLocationSenders.TryGetValue(id, out ActorLocationSender actorMessageSender))
            {
                return;
            }
            self.ActorLocationSenders.Remove(id);
            actorMessageSender.Dispose();
        }
    }
}
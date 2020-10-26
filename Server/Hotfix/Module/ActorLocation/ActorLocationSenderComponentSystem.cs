using System;


namespace ET
{
    public class ActorLocationSenderComponentAwakeSystem : AwakeSystem<ActorLocationSenderComponent>
    {
        public override void Awake(ActorLocationSenderComponent self)
        {
            ActorLocationSenderComponent.Instance = self;
            
            // 每10s扫描一次过期的actorproxy进行回收,过期时间是1分钟
            // 可能由于bug或者进程挂掉，导致ActorLocationSender发送的消息没有确认，结果无法自动删除，每一分钟清理一次这种ActorLocationSender
            self.CheckTimer = TimerComponent.Instance.NewRepeatedTimer(10 * 1000, self.Check);
        }
    }
    
    public class ActorLocationSenderComponentDestroySystem: DestroySystem<ActorLocationSenderComponent>
    {
        public override void Destroy(ActorLocationSenderComponent self)
        {
            ActorLocationSenderComponent.Instance = null;
            TimerComponent.Instance.Remove(self.CheckTimer);
            self.CheckTimer = 0;
        }
    }
    
    public static class ActorLocationSenderComponentSystem
    {
        public static void Check(this ActorLocationSenderComponent self, bool isTimeOut)
        {
            using (ListComponent<long> list = EntityFactory.Create<ListComponent<long>>(self.Domain))
            {
                long timeNow = TimeHelper.Now();
                foreach ((long key, Entity value) in self.Children)
                {
                    ActorLocationSender actorLocationMessageSender = (ActorLocationSender) value;

                    if (timeNow > actorLocationMessageSender.LastSendOrRecvTime + ActorLocationSenderComponent.TIMEOUT_TIME)
                    {
                        list.List.Add(key);
                    }
                }

                foreach (long id in list.List)
                {
                    self.Remove(id);
                }
            }
        }
        
        private static ActorLocationSender Get(this ActorLocationSenderComponent self, long id)
        {
            if (id == 0)
            {
                throw new Exception($"actor id is 0");
            }
            if (self.Children.TryGetValue(id, out Entity actorLocationSender))
            {
                return (ActorLocationSender)actorLocationSender;
            }
			
            actorLocationSender = EntityFactory.CreateWithId<ActorLocationSender>(self.Domain, id);
            actorLocationSender.Parent = self;
            return (ActorLocationSender)actorLocationSender;
        }
		
        private static void Remove(this ActorLocationSenderComponent self, long id)
        {
            if (!self.Children.TryGetValue(id, out Entity actorMessageSender))
            {
                return;
            }
            actorMessageSender.Dispose();
        }
        
        public static void Send(this ActorLocationSenderComponent self, long entityId, IActorLocationMessage message)
        {
            ActorLocationSender actorLocationSender = self.Get(entityId);
            actorLocationSender.Send(message);
        }
		
        public static async ETTask<IActorResponse> Call(this ActorLocationSenderComponent self, long entityId, IActorLocationRequest message)
        {
            ActorLocationSender actorLocationSender = self.Get(entityId);
            return await actorLocationSender.Call(message);
        }
    }
}
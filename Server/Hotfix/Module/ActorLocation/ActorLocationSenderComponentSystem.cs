using System;
using System.IO;

namespace ET
{
    [Timer(TimerType.ActorLocationSenderChecker)]
    public class ActorLocationSenderChecker: ATimer<ActorLocationSenderComponent>
    {
        public override void Run(ActorLocationSenderComponent self)
        {
            try
            {
                self.Check();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }
    
    [ObjectSystem]
    public class ActorLocationSenderComponentAwakeSystem: AwakeSystem<ActorLocationSenderComponent>
    {
        public override void Awake(ActorLocationSenderComponent self)
        {
            ActorLocationSenderComponent.Instance = self;

            // 每10s扫描一次过期的actorproxy进行回收,过期时间是2分钟
            // 可能由于bug或者进程挂掉，导致ActorLocationSender发送的消息没有确认，结果无法自动删除，每一分钟清理一次这种ActorLocationSender
            self.CheckTimer = TimerComponent.Instance.NewRepeatedTimer(10 * 1000, TimerType.ActorLocationSenderChecker, self);
        }
    }

    [ObjectSystem]
    public class ActorLocationSenderComponentDestroySystem: DestroySystem<ActorLocationSenderComponent>
    {
        public override void Destroy(ActorLocationSenderComponent self)
        {
            ActorLocationSenderComponent.Instance = null;
            TimerComponent.Instance.Remove(ref self.CheckTimer);
        }
    }

    public static class ActorLocationSenderComponentSystem
    {
        public static void Check(this ActorLocationSenderComponent self)
        {
            using (ListComponent<long> list = ListComponent<long>.Create())
            {
                long timeNow = TimeHelper.ServerNow();
                foreach ((long key, Entity value) in self.Children)
                {
                    ActorLocationSender actorLocationMessageSender = (ActorLocationSender) value;

                    if (timeNow > actorLocationMessageSender.LastSendOrRecvTime + ActorLocationSenderComponent.TIMEOUT_TIME)
                    {
                        list.Add(key);
                    }
                }

                foreach (long id in list)
                {
                    self.Remove(id);
                }
            }
        }

        private static ActorLocationSender GetOrCreate(this ActorLocationSenderComponent self, long id)
        {
            if (id == 0)
            {
                throw new Exception($"actor id is 0");
            }

            if (self.Children.TryGetValue(id, out Entity actorLocationSender))
            {
                return (ActorLocationSender) actorLocationSender;
            }

            actorLocationSender = self.AddChildWithId<ActorLocationSender>(id);
            return (ActorLocationSender) actorLocationSender;
        }

        private static void Remove(this ActorLocationSenderComponent self, long id)
        {
            if (!self.Children.TryGetValue(id, out Entity actorMessageSender))
            {
                return;
            }

            actorMessageSender.Dispose();
        }

        public static void Send(this ActorLocationSenderComponent self, long entityId, IActorRequest message)
        {
            self.Call(entityId, message).Coroutine();
        }

        public static async ETTask<IActorResponse> Call(this ActorLocationSenderComponent self, long entityId, IActorRequest iActorRequest)
        {
            ActorLocationSender actorLocationSender = self.GetOrCreate(entityId);

            // 先序列化好
            int rpcId = ActorMessageSenderComponent.Instance.GetRpcId();
            iActorRequest.RpcId = rpcId;
            (ushort _, MemoryStream stream) = MessageSerializeHelper.MessageToStream(0, iActorRequest);
            
            long actorLocationSenderInstanceId = actorLocationSender.InstanceId;
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.ActorLocationSender, entityId))
            {
                if (actorLocationSender.InstanceId != actorLocationSenderInstanceId)
                {
                    throw new RpcException(ErrorCore.ERR_ActorTimeout, $"{stream.ToActorMessage()}");
                }

                // 队列中没处理的消息返回跟上个消息一样的报错
                if (actorLocationSender.Error == ErrorCore.ERR_NotFoundActor)
                {
                    return ActorHelper.CreateResponse(iActorRequest, actorLocationSender.Error);
                }
                
                try
                {
                    return await self.CallInner(actorLocationSender, rpcId, stream);
                }
                catch (RpcException)
                {
                    self.Remove(actorLocationSender.Id);
                    throw;
                }
                catch (Exception e)
                {
                    self.Remove(actorLocationSender.Id);
                    throw new Exception($"{stream.ToActorMessage()}", e);
                }
            }
        }

        private static async ETTask<IActorResponse> CallInner(this ActorLocationSenderComponent self, ActorLocationSender actorLocationSender, int rpcId, MemoryStream memoryStream)
        {
            int failTimes = 0;
            long instanceId = actorLocationSender.InstanceId;
            actorLocationSender.LastSendOrRecvTime = TimeHelper.ServerNow();
            
            while (true)
            {
                if (actorLocationSender.ActorId == 0)
                {
                    actorLocationSender.ActorId = await LocationProxyComponent.Instance.Get(actorLocationSender.Id);
                    if (actorLocationSender.InstanceId != instanceId)
                    {
                        throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout2, $"{memoryStream.ToActorMessage()}");
                    }
                }

                if (actorLocationSender.ActorId == 0)
                {
                    IActorRequest iActorRequest = (IActorRequest)memoryStream.ToActorMessage();
                    return ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                }

                IActorResponse response = await ActorMessageSenderComponent.Instance.Call(actorLocationSender.ActorId, rpcId, memoryStream, false);
                if (actorLocationSender.InstanceId != instanceId)
                {
                    throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout3, $"{memoryStream.ToActorMessage()}");
                }

                switch (response.Error)
                {
                    case ErrorCore.ERR_NotFoundActor:
                    {
                        // 如果没找到Actor,重试
                        ++failTimes;
                        if (failTimes > 20)
                        {
                            Log.Debug($"actor send message fail, actorid: {actorLocationSender.Id}");
                            actorLocationSender.Error = ErrorCore.ERR_NotFoundActor;
                            // 这里不能删除actor，要让后面等待发送的消息也返回ERR_NotFoundActor，直到超时删除
                            return response;
                        }

                        // 等待0.5s再发送
                        await TimerComponent.Instance.WaitAsync(500);
                        if (actorLocationSender.InstanceId != instanceId)
                        {
                            throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout4, $"{memoryStream.ToActorMessage()}");
                        }

                        actorLocationSender.ActorId = 0;
                        continue;
                    }
                    case ErrorCore.ERR_ActorTimeout:
                    {
                        throw new RpcException(response.Error, $"{memoryStream.ToActorMessage()}");
                    }
                }

                if (ErrorCore.IsRpcNeedThrowException(response.Error))
                {
                    throw new RpcException(response.Error, $"Message: {response.Message} Request: {memoryStream.ToActorMessage()}");
                }

                return response;
            }
        }
    }
}
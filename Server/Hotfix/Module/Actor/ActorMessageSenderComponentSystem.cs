using System;
using System.IO;

namespace ET
{
    [ObjectSystem]
    public class ActorMessageSenderComponentAwakeSystem: AwakeSystem<ActorMessageSenderComponent>
    {
        public override void Awake(ActorMessageSenderComponent self)
        {
            ActorMessageSenderComponent.Instance = self;

            self.TimeoutCheckTimer = TimerComponent.Instance.NewRepeatedTimer(1000, self.Check);
        }
    }

    [ObjectSystem]
    public class ActorMessageSenderComponentDestroySystem: DestroySystem<ActorMessageSenderComponent>
    {
        public override void Destroy(ActorMessageSenderComponent self)
        {
            ActorMessageSenderComponent.Instance = null;
            TimerComponent.Instance.Remove(ref self.TimeoutCheckTimer);
            self.TimeoutCheckTimer = 0;
            self.TimeoutActorMessageSenders.Clear();
        }
    }

    public static class ActorMessageSenderComponentSystem
    {
        public static void Run(ActorMessageSender self, IActorResponse response)
        {
            if (response.Error == ErrorCode.ERR_ActorTimeout)
            {
                self.Tcs.SetException(new Exception($"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: actorId: {self.ActorId} {self.MemoryStream.ToActorMessage()}, response: {response}"));
                return;
            }

            if (self.NeedException && ErrorCode.IsRpcNeedThrowException(response.Error))
            {
                self.Tcs.SetException(new Exception($"Rpc error: actorId: {self.ActorId} request: {self.MemoryStream.ToActorMessage()}, response: {response}"));
                return;
            }

            self.Tcs.SetResult(response);
        }
        
        public static void Check(this ActorMessageSenderComponent self)
        {
            long timeNow = TimeHelper.ServerNow();
            foreach ((int key, ActorMessageSender value) in self.requestCallback)
            {
                // 因为是顺序发送的，所以，检测到第一个不超时的就退出
                if (timeNow < value.CreateTime + ActorMessageSenderComponent.TIMEOUT_TIME)
                {
                    break;
                }

                self.TimeoutActorMessageSenders.Add(key);
            }

            foreach (int rpcId in self.TimeoutActorMessageSenders)
            {
                ActorMessageSender actorMessageSender = self.requestCallback[rpcId];
                self.requestCallback.Remove(rpcId);
                try
                {
                    IActorResponse response = ActorHelper.CreateResponse((IActorRequest)actorMessageSender.MemoryStream.ToActorMessage(), ErrorCode.ERR_ActorTimeout);
                    Run(actorMessageSender, response);
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }

            self.TimeoutActorMessageSenders.Clear();
        }

        public static void Send(this ActorMessageSenderComponent self, long actorId, IMessage message)
        {
            if (actorId == 0)
            {
                throw new Exception($"actor id is 0: {message}");
            }
            
            ProcessActorId processActorId = new ProcessActorId(actorId);
            Session session = NetInnerComponent.Instance.Get(processActorId.Process);
            session.Send(processActorId.ActorId, message);
        }
        
        public static void Send(this ActorMessageSenderComponent self, long actorId, MemoryStream memoryStream)
        {
            if (actorId == 0)
            {
                throw new Exception($"actor id is 0: {memoryStream.ToActorMessage()}");
            }
            
            ProcessActorId processActorId = new ProcessActorId(actorId);
            Session session = NetInnerComponent.Instance.Get(processActorId.Process);
            session.Send(processActorId.ActorId, memoryStream);
        }


        public static int GetRpcId(this ActorMessageSenderComponent self)
        {
            return ++self.RpcId;
        }

        public static async ETTask<IActorResponse> Call(
                this ActorMessageSenderComponent self,
                long actorId,
                IActorRequest request,
                bool needException = true
        )
        {
            request.RpcId = self.GetRpcId();
            
            if (actorId == 0)
            {
                throw new Exception($"actor id is 0: {request}");
            }

            (ushort _, MemoryStream stream) = MessageSerializeHelper.MessageToStream(0, request);

            return await self.Call(actorId, request.RpcId, stream, needException);
        }
        
        public static async ETTask<IActorResponse> Call(
                this ActorMessageSenderComponent self,
                long actorId,
                int rpcId,
                MemoryStream memoryStream,
                bool needException = true
        )
        {
            if (actorId == 0)
            {
                throw new Exception($"actor id is 0: {memoryStream.ToActorMessage()}");
            }

            var tcs = new ETTaskCompletionSource<IActorResponse>();
            
            self.requestCallback.Add(rpcId, new ActorMessageSender(actorId, memoryStream, tcs, needException));
            
            self.Send(actorId, memoryStream);

            long beginTime = TimeHelper.ServerFrameTime();
            IActorResponse response = await tcs.Task;
            long endTime = TimeHelper.ServerFrameTime();

            long costTime = endTime - beginTime;
            if (costTime > 200)
            {
                Log.Warning("actor rpc time > 200: {0} {1}", costTime, memoryStream.ToActorMessage());
            }
            
            return response;
        }

        public static void RunMessage(this ActorMessageSenderComponent self, long actorId, IActorResponse response)
        {
            ActorMessageSender actorMessageSender;
            if (!self.requestCallback.TryGetValue(response.RpcId, out actorMessageSender))
            {
                return;
            }

            self.requestCallback.Remove(response.RpcId);
            
            Run(actorMessageSender, response);
        }
    }
}
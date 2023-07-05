using System;

namespace ET.Server
{
    public static class ActorOuterComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ActorOuterComponent self)
        {
            self.NetInnerComponent = self.Root().GetComponent<NetInnerComponent>();
        }
        
        public static void HandleIActorResponse(this ActorOuterComponent self, IActorResponse response)
        {
            ActorMessageSender actorMessageSender;
            if (!self.requestCallback.TryGetValue(response.RpcId, out actorMessageSender))
            {
                return;
            }

            self.requestCallback.Remove(response.RpcId);
            
            Run(actorMessageSender, response);
        }
        
        private static void Run(ActorMessageSender self, IActorResponse response)
        {
            if (response.Error == ErrorCore.ERR_ActorTimeout)
            {
                self.Tcs.SetException(new Exception($"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: actorId: {self.ActorId} {self.Request}, response: {response}"));
                return;
            }

            if (self.NeedException && ErrorCore.IsRpcNeedThrowException(response.Error))
            {
                self.Tcs.SetException(new Exception($"Rpc error: actorId: {self.ActorId} request: {self.Request}, response: {response}"));
                return;
            }

            self.Tcs.SetResult(response);
        }
        
        public static void Reply(this ActorOuterComponent self, Address fromAddress, IActorResponse message)
        {
            self.SendInner(new ActorId(fromAddress, 0), message as MessageObject);
        }

        public static void Send(this ActorOuterComponent self, ActorId actorId, IActorMessage message)
        {
            self.SendInner(actorId, message as MessageObject);
        }

        private static void SendInner(this ActorOuterComponent self, ActorId actorId, MessageObject message)
        {
            if (actorId == default)
            {
                throw new Exception($"actor id is 0: {message}");
            }

            Fiber fiber = self.Fiber();
            // 如果发向同一个进程，则报错
            if (actorId.Process == fiber.Process)
            {
                throw new Exception($"actor is the same process: {fiber.Process} {actorId.Process}");
            }
            
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.NetInners[actorId.Process];
            Session session = self.NetInnerComponent.GetComponent<NetInnerComponent>().Get(startSceneConfig.Process);
            actorId.Process = fiber.Process;
            session.Send(actorId, message);
        }

        private static int GetRpcId(this ActorOuterComponent self)
        {
            return ++self.RpcId;
        }

        public static async ETTask<IActorResponse> Call(this ActorOuterComponent self, ActorId actorId, IActorRequest iActorRequest, bool needException = true)
        {
            if (actorId == default)
            {
                throw new Exception($"actor id is 0: {iActorRequest}");
            }

            int rpcId = self.GetRpcId();
            
            var tcs = ETTask<IActorResponse>.Create(true);

            Fiber fiber = self.Fiber();
            self.requestCallback.Add(self.RpcId, new ActorMessageSender(actorId, iActorRequest, tcs, needException));
            
            self.SendInner(actorId, iActorRequest as MessageObject);
            
            async ETTask Timeout()
            {
                await fiber.TimerComponent.WaitAsync(ActorOuterComponent.TIMEOUT_TIME);
                if (!self.requestCallback.TryGetValue(rpcId, out ActorMessageSender action))
                {
                    return;
                }

                self.requestCallback.Remove(rpcId);
                action.Tcs.SetException(new Exception($"actor sender timeout: {iActorRequest}"));
            }
            
            Timeout().Coroutine();
            
            long beginTime = fiber.TimeInfo.ServerFrameTime();

            IActorResponse response = await tcs;
            
            long endTime = fiber.TimeInfo.ServerFrameTime();

            long costTime = endTime - beginTime;
            if (costTime > 200)
            {
                Log.Warning($"actor rpc time > 200: {costTime} {iActorRequest}");
            }
            
            return response;
        }
    }
}
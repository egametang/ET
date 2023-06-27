using System;
using System.IO;

namespace ET
{
    [FriendOf(typeof(ActorSenderComponent))]
    public static partial class ActorSenderComponentSystem
    {
        [Invoke(TimerInvokeType.ActorMessageSenderChecker)]
        public class ActorMessageSenderChecker: ATimer<ActorSenderComponent>
        {
            protected override void Run(ActorSenderComponent self)
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
    
        [EntitySystem]
        private static void Awake(this ActorSenderComponent self)
        {
            self.TimeoutCheckTimer = self.Fiber().GetComponent<TimerComponent>().NewRepeatedTimer(1000, TimerInvokeType.ActorMessageSenderChecker, self);
        }
        
        [EntitySystem]
        private static void Destroy(this ActorSenderComponent self)
        {
            if (self.Fiber().InstanceId == 0)
            {
                return;
            }
            self.Fiber().GetComponent<TimerComponent>().Remove(ref self.TimeoutCheckTimer);
            self.TimeoutCheckTimer = 0;
            self.TimeoutActorMessageSenders.Clear();
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

        private static void Check(this ActorSenderComponent self)
        {
            long timeNow = TimeHelper.ServerNow();
            foreach ((int key, ActorMessageSender value) in self.requestCallback)
            {
                // 因为是顺序发送的，所以，检测到第一个不超时的就退出
                if (timeNow < value.CreateTime + ActorSenderComponent.TIMEOUT_TIME)
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
                    IActorResponse response = ActorHelper.CreateResponse(actorMessageSender.Request, ErrorCore.ERR_ActorTimeout);
                    Run(actorMessageSender, response);
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }

            self.TimeoutActorMessageSenders.Clear();
        }
        
        public static void Reply(this ActorSenderComponent self, Address fromAddress, IMessage message)
        {
            self.Send(new ActorId(fromAddress, 0), message);
        }

        public static void Send(this ActorSenderComponent self, ActorId actorId, IMessage message)
        {
            if (actorId == default)
            {
                throw new Exception($"actor id is 0: {message}");
            }
            
            // 如果发向同一个进程，则扔到消息队列中
            if (actorId.Process == self.Fiber().Process)
            {
                ActorMessageQueue.Instance.Send(self.Fiber().Address, actorId, message as MessageObject);
                return;
            }
            
            // 扔到NetInner纤程
        }



        public static int GetRpcId(this ActorSenderComponent self)
        {
            return ++self.RpcId;
        }

        public static async ETTask<IActorResponse> Call(
                this ActorSenderComponent self,
                ActorId actorId,
                IActorRequest request,
                bool needException = true
        )
        {
            request.RpcId = self.GetRpcId();
            
            if (actorId == default)
            {
                throw new Exception($"actor id is 0: {request}");
            }

            return await self.Call(actorId, request.RpcId, request, needException);
        }
        
        public static async ETTask<IActorResponse> Call(
                this ActorSenderComponent self,
                ActorId actorId,
                int rpcId,
                IActorRequest iActorRequest,
                bool needException = true
        )
        {
            if (actorId == default)
            {
                throw new Exception($"actor id is 0: {iActorRequest}");
            }

            var tcs = ETTask<IActorResponse>.Create(true);
            
            self.requestCallback.Add(rpcId, new ActorMessageSender(actorId, iActorRequest, tcs, needException));
            
            self.Send(actorId, iActorRequest);

            long beginTime = TimeHelper.ServerFrameTime();
            IActorResponse response = await tcs;
            long endTime = TimeHelper.ServerFrameTime();

            long costTime = endTime - beginTime;
            if (costTime > 200)
            {
                Log.Warning($"actor rpc time > 200: {costTime} {iActorRequest}");
            }
            
            return response;
        }

        public static void HandleIActorResponse(this ActorSenderComponent self, IActorResponse response)
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
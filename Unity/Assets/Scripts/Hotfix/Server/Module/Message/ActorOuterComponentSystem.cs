using System;
using System.Net;

namespace ET.Server
{
    [EntitySystemOf(typeof(ActorOuterComponent))]
    [FriendOf(typeof(ActorOuterComponent))]
    public static partial class ActorOuterComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ActorOuterComponent self, IPEndPoint address)
        {
            switch (self.InnerProtocol)
            {
                case NetworkProtocol.TCP:
                {
                    self.AService = new TService(address, ServiceType.Inner);
                    break;
                }
                case NetworkProtocol.KCP:
                {
                    self.AService = new KService(address, ServiceType.Inner);
                    break;
                }
            }
                
            self.AService.AcceptCallback = self.OnAccept;
            self.AService.ReadCallback = self.OnRead;
            self.AService.ErrorCallback = self.OnError;
        }
        
        [EntitySystem]
        private static void Update(this ActorOuterComponent self)
        {
            self.AService.Update();
        }

        [EntitySystem]
        private static void Destroy(this ActorOuterComponent self)
        {
            self.AService.Dispose();
        }

        private static void OnRead(this ActorOuterComponent self, long channelId, ActorId actorId, object message)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }
            
            session.LastRecvTime = self.Fiber().TimeInfo.ClientFrameTime();

            self.HandleMessage(actorId, message).Coroutine();
        }

        private static async ETTask HandleMessage(this ActorOuterComponent self, ActorId actorId, object message)
        {
            Fiber fiber = self.Fiber();
            int fromProcess = actorId.Process;
            actorId.Process = fiber.Process;

            switch (message)
            {
                case IActorResponse iActorResponse:
                {
                    self.HandleIActorResponse(iActorResponse);
                    return;
                }
                case IActorLocationRequest iActorRequest:
                {
                    IActorResponse response = await fiber.ActorInnerComponent.Call(actorId, iActorRequest, false);
                    actorId.Process = fromProcess;
                    self.Send(actorId, response);
                    break;
                }
                case IActorRequest iActorRequest:
                {
                    IActorResponse response = await fiber.ActorInnerComponent.Call(actorId, iActorRequest);
                    actorId.Process = fromProcess;
                    self.Send(actorId, response);
                    break;
                }
                default:
                {
                    ActorMessageQueue.Instance.Send(actorId, (MessageObject)message);
                    break;
                }
            }
        }

        private static void OnError(this ActorOuterComponent self, long channelId, int error)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.Error = error;
            session.Dispose();
        }

        // 这个channelId是由CreateAcceptChannelId生成的
        private static void OnAccept(this ActorOuterComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            session.RemoteAddress = ipEndPoint;
            //session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);
        }

        private static Session CreateInner(this ActorOuterComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            session.RemoteAddress = ipEndPoint;
            self.AService.Create(channelId, ipEndPoint);

            //session.AddComponent<InnerPingComponent>();
            //session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);

            return session;
        }

        // 内网actor session，channelId是进程号
        private static Session Get(this ActorOuterComponent self, long channelId)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session != null)
            {
                return session;
            }

            IPEndPoint ipEndPoint = StartSceneConfigCategory.Instance.Get((int) channelId).InnerIPPort;
            session = self.CreateInner(channelId, ipEndPoint);
            return session;
        }

        public static void HandleIActorResponse(this ActorOuterComponent self, IActorResponse response)
        {
            if (!self.requestCallback.Remove(response.RpcId, out ActorMessageSender actorMessageSender))
            {
                return;
            }
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
            Session session = self.Get(startSceneConfig.Id);
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
            Fiber fiber = self.Fiber();
            
            int rpcId = self.GetRpcId();

            var tcs = ETTask<IActorResponse>.Create(true);

            self.requestCallback.Add(self.RpcId, new ActorMessageSender(actorId, iActorRequest, tcs, needException));

            self.SendInner(actorId, iActorRequest as MessageObject);

            async ETTask Timeout()
            {
                await fiber.TimerComponent.WaitAsync(ActorOuterComponent.TIMEOUT_TIME);
                if (!self.requestCallback.Remove(rpcId, out ActorMessageSender action))
                {
                    return;
                }
                
                if (needException)
                {
                    action.Tcs.SetException(new Exception($"actor sender timeout: {iActorRequest}"));
                }
                else
                {
                    IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_Timeout);
                    action.Tcs.SetResult(response);
                }
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
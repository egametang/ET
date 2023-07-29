using System;
using System.Net;

namespace ET.Server
{
    [EntitySystemOf(typeof(MessageOuterSender))]
    [FriendOf(typeof(MessageOuterSender))]
    public static partial class MessageOuterSenderSystem
    {
        [EntitySystem]
        private static void Awake(this MessageOuterSender self, IPEndPoint address)
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
        private static void Update(this MessageOuterSender self)
        {
            self.AService.Update();
        }

        [EntitySystem]
        private static void Destroy(this MessageOuterSender self)
        {
            self.AService.Dispose();
        }

        private static void OnRead(this MessageOuterSender self, long channelId, ActorId actorId, object message)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }
            
            session.LastRecvTime = TimeInfo.Instance.ClientFrameTime();

            self.HandleMessage(actorId, message).Coroutine();
        }

        private static async ETTask HandleMessage(this MessageOuterSender self, ActorId actorId, object message)
        {
            Fiber fiber = self.Fiber();
            int fromProcess = actorId.Process;
            actorId.Process = fiber.Process;

            switch (message)
            {
                case IResponse iActorResponse:
                {
                    self.HandleIActorResponse(iActorResponse);
                    return;
                }
                case ILocationRequest:
                case IRequest:
                {
                    IRequest request = (IRequest)message;
                    // 注意这里都不能抛异常，因为这里只是中转消息
                    IResponse response = await fiber.MessageInnerSender.Call(actorId, request, false);
                    actorId.Process = fromProcess;
                    self.Send(actorId, response);
                    break;
                }
                default:
                {
                    fiber.MessageInnerSender.Send(actorId, (IMessage)message);
                    break;
                }
            }
        }

        private static void OnError(this MessageOuterSender self, long channelId, int error)
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
        private static void OnAccept(this MessageOuterSender self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            session.RemoteAddress = ipEndPoint;
            //session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);
        }

        private static Session CreateInner(this MessageOuterSender self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            session.RemoteAddress = ipEndPoint;
            self.AService.Create(channelId, ipEndPoint);

            //session.AddComponent<InnerPingComponent>();
            //session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);

            return session;
        }

        // 内网actor session，channelId是进程号
        private static Session Get(this MessageOuterSender self, long channelId)
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

        public static void HandleIActorResponse(this MessageOuterSender self, IResponse response)
        {
            if (!self.requestCallback.Remove(response.RpcId, out MessageSenderStruct actorMessageSender))
            {
                return;
            }
            Run(actorMessageSender, response);
        }

        private static void Run(MessageSenderStruct self, IResponse response)
        {
            if (response.Error == ErrorCore.ERR_MessageTimeout)
            {
                self.Tcs.SetException(new RpcException(response.Error, $"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: actorId: {self.ActorId} {self.Request}, response: {response}"));
                return;
            }

            if (self.NeedException && ErrorCore.IsRpcNeedThrowException(response.Error))
            {
                self.Tcs.SetException(new RpcException(response.Error, $"Rpc error: actorId: {self.ActorId} request: {self.Request}, response: {response}"));
                return;
            }

            self.Tcs.SetResult(response);
            ((MessageObject)response).Dispose();
        }

        public static void Send(this MessageOuterSender self, ActorId actorId, IMessage message)
        {
            self.SendInner(actorId, message as MessageObject);
        }

        private static void SendInner(this MessageOuterSender self, ActorId actorId, MessageObject message)
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

        private static int GetRpcId(this MessageOuterSender self)
        {
            return ++self.RpcId;
        }

        public static async ETTask<IResponse> Call(this MessageOuterSender self, ActorId actorId, IRequest iRequest, bool needException = true)
        {
            if (actorId == default)
            {
                throw new Exception($"actor id is 0: {iRequest}");
            }
            Fiber fiber = self.Fiber();
            
            int rpcId = self.GetRpcId();

            var tcs = ETTask<IResponse>.Create(true);

            self.requestCallback.Add(self.RpcId, new MessageSenderStruct(actorId, iRequest, tcs, needException));

            self.SendInner(actorId, iRequest as MessageObject);

            async ETTask Timeout()
            {
                await fiber.TimerComponent.WaitAsync(MessageOuterSender.TIMEOUT_TIME);
                if (!self.requestCallback.Remove(rpcId, out MessageSenderStruct action))
                {
                    return;
                }
                
                if (needException)
                {
                    action.Tcs.SetException(new Exception($"actor sender timeout: {iRequest}"));
                }
                else
                {
                    IResponse response = ET.MessageHelper.CreateResponse(iRequest, ErrorCore.ERR_Timeout);
                    action.Tcs.SetResult(response);
                }
            }

            Timeout().Coroutine();

            long beginTime = TimeInfo.Instance.ServerFrameTime();

            IResponse response = await tcs;

            long endTime = TimeInfo.Instance.ServerFrameTime();

            long costTime = endTime - beginTime;
            if (costTime > 200)
            {
                Log.Warning($"actor rpc time > 200: {costTime} {iRequest}");
            }

            return response;
        }
    }
}
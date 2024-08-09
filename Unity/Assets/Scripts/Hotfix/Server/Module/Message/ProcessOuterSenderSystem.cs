using System;
using System.Net;

namespace ET.Server
{
    [EntitySystemOf(typeof(ProcessOuterSender))]
    [FriendOf(typeof(ProcessOuterSender))]
    public static partial class ProcessOuterSenderSystem
    {
        [EntitySystem]
        private static void Awake(this ProcessOuterSender self, IPEndPoint address)
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
                    self.AService = new KService(address, NetworkProtocol.UDP, ServiceType.Inner);
                    break;
                }
            }
                
            self.AService.AcceptCallback = self.OnAccept;
            self.AService.ReadCallback = self.OnRead;
            self.AService.ErrorCallback = self.OnError;
        }
        
        
        [EntitySystem]
        private static void Update(this ProcessOuterSender self)
        {
            self.AService.Update();
        }

        [EntitySystem]
        private static void Destroy(this ProcessOuterSender self)
        {
            self.AService.Dispose();
        }

        private static void OnRead(this ProcessOuterSender self, long channelId, MemoryBuffer memoryBuffer)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }
            
            session.LastRecvTime = TimeInfo.Instance.ClientFrameTime();

            (ActorId actorId, object message) = MessageSerializeHelper.ToMessage(self.AService, memoryBuffer);
            self.AService.Recycle(memoryBuffer);
            
            if (message is IResponse response)
            {
                self.HandleIActorResponse(response);
                return;
            }

            Fiber fiber = self.Fiber();
            int fromProcess = actorId.Process;
            actorId.Process = fiber.Process;

            switch (message)
            {
                case ILocationRequest:
                case IRequest:
                {
                    CallInner().Coroutine();
                    break;

                    async ETTask CallInner()
                    {
                        IRequest req = (IRequest)message;
                        int rpcId = req.RpcId;
                        // 注意这里都不能抛异常，因为这里只是中转消息
                        IResponse res = await fiber.Root.GetComponent<ProcessInnerSender>().Call(actorId, req, false);
                        // 注意这里的response会在该协程执行完之后由ProcessInnerSender dispose。
                        actorId.Process = fromProcess;
                        res.RpcId = rpcId;
                        self.Send(actorId, res);
                        ((MessageObject)res).Dispose();
                    }
                }
                default:
                {
                    fiber.Root.GetComponent<ProcessInnerSender>().Send(actorId, (IMessage)message);
                    break;
                }
            }
        }

        private static void OnError(this ProcessOuterSender self, long channelId, int error)
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
        private static void OnAccept(this ProcessOuterSender self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            session.RemoteAddress = ipEndPoint;
            //session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);
        }

        private static Session CreateInner(this ProcessOuterSender self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
            session.RemoteAddress = ipEndPoint;
            self.AService.Create(channelId, session.RemoteAddress);

            //session.AddComponent<InnerPingComponent>();
            //session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);

            return session;
        }

        // 内网actor session，channelId是进程号
        private static Session Get(this ProcessOuterSender self, long channelId)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session != null)
            {
                return session;
            }

            IPEndPoint ipEndPoint = StartProcessConfigCategory.Instance.Get((int) channelId).IPEndPoint;
            session = self.CreateInner(channelId, ipEndPoint);
            return session;
        }

        private static void HandleIActorResponse(this ProcessOuterSender self, IResponse response)
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
                self.SetException(new RpcException(response.Error, $"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: actorId: {self.ActorId} {self.RequestType.FullName}, response: {response}"));
                return;
            }

            if (self.NeedException && ErrorCore.IsRpcNeedThrowException(response.Error))
            {
                self.SetException(new RpcException(response.Error, $"Rpc error: actorId: {self.ActorId} request: {self.RequestType.FullName}, response: {response}"));
                return;
            }

            self.SetResult(response);
        }

        public static void Send(this ProcessOuterSender self, ActorId actorId, IMessage message)
        {
            self.SendInner(actorId, message as MessageObject);
        }

        private static void SendInner(this ProcessOuterSender self, ActorId actorId, MessageObject message)
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
            
            StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(actorId.Process);
            Session session = self.Get(startProcessConfig.Id);
            actorId.Process = fiber.Process;
            session.Send(actorId, message);
        }

        private static int GetRpcId(this ProcessOuterSender self)
        {
            return ++self.RpcId;
        }

        public static async ETTask<IResponse> Call(this ProcessOuterSender self, ActorId actorId, IRequest iRequest, bool needException = true)
        {
            if (actorId == default)
            {
                throw new Exception($"actor id is 0: {iRequest}");
            }
            Fiber fiber = self.Fiber();
            
            int rpcId = self.GetRpcId();

            iRequest.RpcId = rpcId;

            Type requestType = iRequest.GetType();
            MessageSenderStruct messageSenderStruct = new(actorId, requestType, needException);
            self.requestCallback.Add(rpcId, messageSenderStruct);
            
            self.SendInner(actorId, iRequest as MessageObject);

            async ETTask Timeout()
            {
                await fiber.Root.GetComponent<TimerComponent>().WaitAsync(ProcessOuterSender.TIMEOUT_TIME);
                if (!self.requestCallback.Remove(rpcId, out MessageSenderStruct action))
                {
                    return;
                }
                
                if (needException)
                {
                    action.SetException(new Exception($"actor sender timeout: {requestType.FullName}"));
                }
                else
                {
                    IResponse response = MessageHelper.CreateResponse(requestType, rpcId, ErrorCore.ERR_Timeout);
                    action.SetResult(response);
                }
            }

            Timeout().Coroutine();

            long beginTime = TimeInfo.Instance.ServerFrameTime();

            IResponse response = await messageSenderStruct.Wait();

            long endTime = TimeInfo.Instance.ServerFrameTime();

            long costTime = endTime - beginTime;
            if (costTime > 200)
            {
                Log.Warning($"actor rpc time > 200: {costTime} {requestType.FullName}");
            }

            return response;
        }
    }
}
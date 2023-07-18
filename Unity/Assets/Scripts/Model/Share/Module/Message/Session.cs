using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ET
{
    public readonly struct RpcInfo
    {
        public readonly IActorRequest Request;
        public readonly ETTask<IActorResponse> Tcs;

        public RpcInfo(IActorRequest request)
        {
            this.Request = request;
            this.Tcs = ETTask<IActorResponse>.Create(true);
        }
    }
    
    [EntitySystemOf(typeof(Session))]
    [FriendOf(typeof(Session))]
    public static partial class SessionSystem
    {
        [EntitySystem]
        private static void Awake(this Session self, AService aService)
        {
            self.AService = aService;
            long timeNow = TimeInfo.Instance.ClientNow();
            self.LastRecvTime = timeNow;
            self.LastSendTime = timeNow;

            self.requestCallbacks.Clear();
            
            Log.Info($"session create: zone: {self.Zone()} id: {self.Id} {timeNow} ");
        }
        
        [EntitySystem]
        private static void Destroy(this Session self)
        {
            self.AService.Remove(self.Id, self.Error);
            
            foreach (RpcInfo responseCallback in self.requestCallbacks.Values.ToArray())
            {
                responseCallback.Tcs.SetException(new RpcException(self.Error, $"session dispose: {self.Id} {self.RemoteAddress}"));
            }

            Log.Info($"session dispose: {self.RemoteAddress} id: {self.Id} ErrorCode: {self.Error}, please see ErrorCode.cs! {TimeInfo.Instance.ClientNow()}");
            
            self.requestCallbacks.Clear();
        }
        
        public static void OnResponse(this Session self, IActorResponse response)
        {
            if (!self.requestCallbacks.Remove(response.RpcId, out var action))
            {
                return;
            }

            if (ErrorCore.IsRpcNeedThrowException(response.Error))
            {
                action.Tcs.SetException(new Exception($"Rpc error, request: {action.Request} response: {response}"));
                return;
            }
            action.Tcs.SetResult(response);
        }
        
        public static async ETTask<IActorResponse> Call(this Session self, IActorRequest request, ETCancellationToken cancellationToken)
        {
            int rpcId = ++self.RpcId;
            RpcInfo rpcInfo = new RpcInfo(request);
            self.requestCallbacks[rpcId] = rpcInfo;
            request.RpcId = rpcId;

            self.Send(request);
            
            void CancelAction()
            {
                if (!self.requestCallbacks.TryGetValue(rpcId, out RpcInfo action))
                {
                    return;
                }

                self.requestCallbacks.Remove(rpcId);
                Type responseType = OpcodeType.Instance.GetResponseType(action.Request.GetType());
                IActorResponse response = (IActorResponse) Activator.CreateInstance(responseType);
                response.Error = ErrorCore.ERR_Cancel;
                action.Tcs.SetResult(response);
            }

            IActorResponse ret;
            try
            {
                cancellationToken?.Add(CancelAction);
                ret = await rpcInfo.Tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
            return ret;
        }

        public static async ETTask<IActorResponse> Call(this Session self, IActorRequest request, int time = 0)
        {
            int rpcId = ++self.RpcId;
            RpcInfo rpcInfo = new(request);
            self.requestCallbacks[rpcId] = rpcInfo;
            request.RpcId = rpcId;
            self.Send(request);

            if (time > 0)
            {
                async ETTask Timeout()
                {
                    await self.Fiber().TimerComponent.WaitAsync(time);
                    if (!self.requestCallbacks.TryGetValue(rpcId, out RpcInfo action))
                    {
                        return;
                    }

                    if (!self.requestCallbacks.Remove(rpcId))
                    {
                        return;
                    }
                    
                    action.Tcs.SetException(new Exception($"session call timeout: {request} {time}"));
                }
                
                Timeout().Coroutine();
            }

            return await rpcInfo.Tcs;
        }

        public static void Send(this Session self, IActorMessage message)
        {
            self.Send(default, message);
        }
        
        public static void Send(this Session self, ActorId actorId, IActorMessage message)
        {
            self.LastSendTime = TimeInfo.Instance.ClientNow();
            LogMsg.Instance.Debug(message);
            self.AService.Send(self.Id, actorId, message as MessageObject);
        }
    }

    [ChildOf]
    public sealed class Session: Entity, IAwake<AService>, IDestroy
    {
        public AService AService { get; set; }
        
        public int RpcId
        {
            get;
            set;
        }

        public readonly Dictionary<int, RpcInfo> requestCallbacks = new();
        
        public long LastRecvTime
        {
            get;
            set;
        }

        public long LastSendTime
        {
            get;
            set;
        }

        public int Error
        {
            get;
            set;
        }

        public IPEndPoint RemoteAddress
        {
            get;
            set;
        }
    }
}
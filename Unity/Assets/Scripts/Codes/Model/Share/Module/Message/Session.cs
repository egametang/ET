using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ET
{
    public readonly struct RpcInfo
    {
        public readonly IRequest Request;
        public readonly ETTask<IResponse> Tcs;

        public RpcInfo(IRequest request)
        {
            this.Request = request;
            this.Tcs = ETTask<IResponse>.Create(true);
        }
    }
    
    [FriendOf(typeof(Session))]
    public static class SessionSystem
    {
        [ObjectSystem]
        public class SessionAwakeSystem: AwakeSystem<Session, int>
        {
            protected override void Awake(Session self, int serviceId)
            {
                self.ServiceId = serviceId;
                long timeNow = TimeHelper.ClientNow();
                self.LastRecvTime = timeNow;
                self.LastSendTime = timeNow;

                self.requestCallbacks.Clear();
            
                Log.Info($"session create: zone: {self.DomainZone()} id: {self.Id} {timeNow} ");
            }
        }
        
        [ObjectSystem]
        public class SessionDestroySystem: DestroySystem<Session>
        {
            protected override void Destroy(Session self)
            {
                NetServices.Instance.RemoveChannel(self.ServiceId, self.Id, self.Error);
            
                foreach (RpcInfo responseCallback in self.requestCallbacks.Values.ToArray())
                {
                    responseCallback.Tcs.SetException(new RpcException(self.Error, $"session dispose: {self.Id} {self.RemoteAddress}"));
                }

                Log.Info($"session dispose: {self.RemoteAddress} id: {self.Id} ErrorCode: {self.Error}, please see ErrorCode.cs! {TimeHelper.ClientNow()}");
            
                self.requestCallbacks.Clear();
            }
        }
        
        public static void OnResponse(this Session self, IResponse response)
        {
            if (!self.requestCallbacks.TryGetValue(response.RpcId, out var action))
            {
                return;
            }

            self.requestCallbacks.Remove(response.RpcId);
            if (ErrorCore.IsRpcNeedThrowException(response.Error))
            {
                action.Tcs.SetException(new Exception($"Rpc error, request: {action.Request} response: {response}"));
                return;
            }
            action.Tcs.SetResult(response);
        }
        
        public static async ETTask<IResponse> Call(this Session self, IRequest request, ETCancellationToken cancellationToken)
        {
            int rpcId = ++Session.RpcId;
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
                Type responseType = OpcodeTypeComponent.Instance.GetResponseType(action.Request.GetType());
                IResponse response = (IResponse) Activator.CreateInstance(responseType);
                response.Error = ErrorCore.ERR_Cancel;
                action.Tcs.SetResult(response);
            }

            IResponse ret;
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

        public static async ETTask<IResponse> Call(this Session self, IRequest request)
        {
            int rpcId = ++Session.RpcId;
            RpcInfo rpcInfo = new RpcInfo(request);
            self.requestCallbacks[rpcId] = rpcInfo;
            request.RpcId = rpcId;
            self.Send(request);
            return await rpcInfo.Tcs;
        }

        public static void Send(this Session self, IMessage message)
        {
            self.Send(0, message);
        }
        
        public static void Send(this Session self, long actorId, IMessage message)
        {
            self.LastSendTime = TimeHelper.ClientNow();
            OpcodeHelper.LogMsg(self.DomainZone(), message);
            NetServices.Instance.SendMessage(self.ServiceId, self.Id, actorId, message);
        }
    }

    [ChildOf]
    public sealed class Session: Entity, IAwake<int>, IDestroy
    {
        public int ServiceId { get; set; }
        
        public static int RpcId
        {
            get;
            set;
        }

        public readonly Dictionary<int, RpcInfo> requestCallbacks = new Dictionary<int, RpcInfo>();
        
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
using System;
using System.Linq;

namespace ET
{
    [EntitySystemOf(typeof(Session))]
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
                responseCallback.SetException(new RpcException(self.Error, $"session dispose: {self.Id} {self.RemoteAddress}"));
            }

            Log.Info($"session dispose: {self.RemoteAddress} id: {self.Id} ErrorCode: {self.Error}, please see ErrorCode.cs! {TimeInfo.Instance.ClientNow()}");
            
            self.requestCallbacks.Clear();
        }
        
        public static void OnResponse(this Session self, IResponse response)
        {
            if (!self.requestCallbacks.Remove(response.RpcId, out RpcInfo action))
            {
                return;
            }
            action.SetResult(response);
        }

        public static async ETTask<IResponse> Call(this Session self, IRequest request)
        {
            int rpcId = ++self.RpcId;
            RpcInfo rpcInfo = new(request.GetType());
            self.requestCallbacks[rpcId] = rpcInfo;
            
            request.RpcId = rpcId;

            self.Send(request);
            
            void CancelAction()
            {
                if (!self.requestCallbacks.Remove(rpcId, out RpcInfo action))
                {
                    return;
                }

                Type responseType = OpcodeType.Instance.GetResponseType(action.RequestType);
                IResponse response = (IResponse) Activator.CreateInstance(responseType);
                response.Error = ErrorCode.ERR_Cancel;
                action.SetResult(response);
            }

            ETCancellationToken cancellationToken = await ETTaskHelper.GetContextAsync<ETCancellationToken>();
            IResponse ret;
            try
            {
                cancellationToken?.Add(CancelAction);
                ret = await rpcInfo.Wait();
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
            return ret;
        }

        public static async ETTask<IResponse> Call(this Session self, IRequest request, int timeout)
        {
            return await self.Call(request).TimeoutAsync(timeout);
        }

        public static void Send(this Session self, IMessage message)
        {
            self.Send(default, message);
        }
        
        public static void Send(this Session self, ActorId actorId, IMessage message)
        {
            self.LastSendTime = TimeInfo.Instance.ClientNow();
            LogMsg.Instance.Debug(self.Fiber(), message);

            (ushort opcode, MemoryBuffer memoryBuffer) = MessageSerializeHelper.ToMemoryBuffer(self.AService, actorId, message);
            self.AService.Send(self.Id, memoryBuffer);
        }
    }
}
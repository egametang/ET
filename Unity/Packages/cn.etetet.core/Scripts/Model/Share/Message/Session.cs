using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ET
{
    public readonly struct RpcInfo
    {
        public Type RequestType { get; }
        
        private readonly ETTask<IResponse> tcs;

        public RpcInfo(Type requestType)
        {
            this.RequestType = requestType;
            
            this.tcs = ETTask<IResponse>.Create(true);
        }

        public void SetResult(IResponse response)
        {
            this.tcs.SetResult(response);
        }

        public void SetException(Exception exception)
        {
            this.tcs.SetException(exception);
        }

        public async ETTask<IResponse> Wait()
        {
            return await this.tcs;
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
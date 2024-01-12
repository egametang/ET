using System;

namespace ET
{
    public static class MessageHelper
    {
        public static IResponse CreateResponse(Type requestType, int rpcId, int error)
        {
            Type responseType = OpcodeType.Instance.GetResponseType(requestType);
            IResponse response = (IResponse)ObjectPool.Instance.Fetch(responseType);
            response.Error = error;
            response.RpcId = rpcId;
            return response;
        }
    }
}
using System;

namespace ET
{
    public static class MessageHelper
    {
        public static IResponse CreateResponse(IRequest iRequest, int error)
        {
            Type responseType = OpcodeType.Instance.GetResponseType(iRequest.GetType());
            IResponse response = (IResponse)ObjectPool.Instance.Fetch(responseType);
            response.Error = error;
            response.RpcId = iRequest.RpcId;
            return response;
        }
    }
}
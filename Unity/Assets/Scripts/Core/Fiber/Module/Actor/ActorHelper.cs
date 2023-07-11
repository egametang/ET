using System;

namespace ET
{
    public static class ActorHelper
    {
        public static IActorResponse CreateResponse(IActorRequest iActorRequest, int error)
        {
            Type responseType = OpcodeType.Instance.GetResponseType(iActorRequest.GetType());
            IActorResponse response = (IActorResponse)ObjectPool.Instance.Fetch(responseType);
            response.Error = error;
            response.RpcId = iActorRequest.RpcId;
            return response;
        }
    }
}
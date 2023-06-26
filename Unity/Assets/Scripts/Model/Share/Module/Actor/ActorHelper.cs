using System;

namespace ET
{
    public static partial class ActorHelper
    {
        public static IActorResponse CreateResponse(IActorRequest iActorRequest, int error)
        {
            Type responseType = OpcodeType.Instance.GetResponseType(iActorRequest.GetType());
            IActorResponse response = (IActorResponse)Activator.CreateInstance(responseType);
            response.Error = error;
            response.RpcId = iActorRequest.RpcId;
            return response;
        }
    }
}
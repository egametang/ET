using System;

namespace ET
{
    [ActorMessageHandler]
    public class ObjectGetRequestHandler: AMActorRpcHandler<Scene, ObjectGetRequest, ObjectGetResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectGetRequest request, ObjectGetResponse response, Action reply)
        {
            long instanceId = await scene.GetComponent<LocationComponent>().Get(request.Key);
            response.InstanceId = instanceId;
            reply();
        }
    }
}
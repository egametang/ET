using System;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Location)]
    public class ObjectGetRequestHandler: AMActorRpcHandler<Scene, ObjectGetRequest, ObjectGetResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectGetRequest request, ObjectGetResponse response)
        {
            long instanceId = await scene.GetComponent<LocationManagerComoponent>().Get(request.Type).Get(request.Key);
            response.InstanceId = instanceId;
        }
    }
}
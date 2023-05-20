using System;

namespace ET.Server
{
    public static partial class ObjectGetRequestHandler
    {
        [ActorMessageHandler(SceneType.Location)]
        private static async ETTask Run(Scene scene, ObjectGetRequest request, ObjectGetResponse response)
        {
            long instanceId = await scene.GetComponent<LocationManagerComoponent>().Get(request.Type).Get(request.Key);
            response.InstanceId = instanceId;
        }
    }
}
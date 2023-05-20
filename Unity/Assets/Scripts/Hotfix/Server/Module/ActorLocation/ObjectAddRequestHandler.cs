using System;

namespace ET.Server
{
    public static partial class ObjectAddRequestHandler
    {
        [ActorMessageHandler(SceneType.Location)]
        private static async ETTask Run(Scene scene, ObjectAddRequest request, ObjectAddResponse response)
        {
            await scene.GetComponent<LocationManagerComoponent>().Get(request.Type).Add(request.Key, request.InstanceId);
        }
    }
}
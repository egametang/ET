using System;

namespace ET.Server
{
    public static partial class ObjectLockRequestHandler
    {
        [ActorMessageHandler(SceneType.Location)]
        private static async ETTask Run(Scene scene, ObjectLockRequest request, ObjectLockResponse response)
        {
            await scene.GetComponent<LocationManagerComoponent>().Get(request.Type).Lock(request.Key, request.InstanceId, request.Time);
        }
    }
}
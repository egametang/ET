using System;

namespace ET.Server
{
    public static partial class ObjectUnLockRequestHandler
    {
        [ActorMessageHandler(SceneType.Location)]
        private static async ETTask Run(Scene scene, ObjectUnLockRequest request, ObjectUnLockResponse response)
        {
            scene.GetComponent<LocationManagerComoponent>().Get(request.Type).UnLock(request.Key, request.OldInstanceId, request.InstanceId);

            await ETTask.CompletedTask;
        }
    }
}
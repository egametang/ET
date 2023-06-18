using System;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Location)]
    public class ObjectUnLockRequestHandler: ActorMessageHandler<Scene, ObjectUnLockRequest, ObjectUnLockResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectUnLockRequest request, ObjectUnLockResponse response)
        {
            scene.GetComponent<LocationManagerComoponent>().Get(request.Type).UnLock(request.Key, request.OldActorId, request.NewActorId);

            await ETTask.CompletedTask;
        }
    }
}
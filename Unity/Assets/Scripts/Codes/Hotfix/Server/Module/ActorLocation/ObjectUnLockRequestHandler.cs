using System;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Location)]
    public class ObjectUnLockRequestHandler: AMActorRpcHandler<Scene, ObjectUnLockRequest, ObjectUnLockResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectUnLockRequest request, ObjectUnLockResponse response)
        {
            scene.GetComponent<LocationComponent>().UnLock(request.Key, request.OldInstanceId, request.InstanceId);

            await ETTask.CompletedTask;
        }
    }
}
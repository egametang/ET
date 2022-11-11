using System;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Location)]
    public class ObjectLockRequestHandler: AMActorRpcHandler<Scene, ObjectLockRequest, ObjectLockResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectLockRequest request, ObjectLockResponse response, Action reply)
        {
            await scene.GetComponent<LocationComponent>().Lock(request.Key, request.InstanceId, request.Time);

            reply();

            await ETTask.CompletedTask;
        }
    }
}
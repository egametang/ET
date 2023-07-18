using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectAddRequestHandler: MessageHandler<Scene, ObjectAddRequest, ObjectAddResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectAddRequest request, ObjectAddResponse response)
        {
            await scene.GetComponent<LocationManagerComoponent>().Get(request.Type).Add(request.Key, request.ActorId);
        }
    }
}
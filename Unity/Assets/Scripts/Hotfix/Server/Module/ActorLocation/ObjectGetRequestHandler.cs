using System;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Location)]
    public class ObjectGetRequestHandler: ActorMessageHandler<Scene, ObjectGetRequest, ObjectGetResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectGetRequest request, ObjectGetResponse response)
        {
            response.ActorId = await scene.GetComponent<LocationManagerComoponent>().Get(request.Type).Get(request.Key);
        }
    }
}
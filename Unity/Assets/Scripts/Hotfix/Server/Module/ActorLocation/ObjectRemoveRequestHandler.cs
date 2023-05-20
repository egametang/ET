using System;

namespace ET.Server
{
    public static partial class ObjectRemoveRequestHandler
    {
        [ActorMessageHandler(SceneType.Location)]
        private static async ETTask Run(Scene scene, ObjectRemoveRequest request, ObjectRemoveResponse response)
        {
            await scene.GetComponent<LocationManagerComoponent>().Get(request.Type).Remove(request.Key);
        }
    }
}
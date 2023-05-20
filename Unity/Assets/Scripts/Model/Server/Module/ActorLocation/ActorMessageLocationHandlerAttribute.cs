using System;

namespace ET.Server
{
    public class ActorMessageLocationHandlerAttribute: ActorMessageHandlerAttribute
    {
        public ActorMessageLocationHandlerAttribute(SceneType sceneType): base(sceneType)
        {
        }
    }
}
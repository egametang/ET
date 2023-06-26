using System;

namespace ET
{
    public class ActorMessageLocationHandlerAttribute: ActorMessageHandlerAttribute
    {
        public ActorMessageLocationHandlerAttribute(SceneType sceneType): base(sceneType)
        {
        }
    }
}
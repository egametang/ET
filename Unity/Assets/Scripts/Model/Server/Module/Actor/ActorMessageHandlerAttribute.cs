using System;

namespace ET.Server
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ActorMessageHandlerAttribute: BaseAttribute
    {
        public SceneType SceneType { get; }

        public ActorMessageHandlerAttribute(SceneType sceneType)
        {
            this.SceneType = sceneType;
        }
    }
}
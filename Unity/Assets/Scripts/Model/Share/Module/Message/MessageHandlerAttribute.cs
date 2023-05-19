using System;

namespace ET
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MessageHandlerAttribute: BaseAttribute
    {
        public SceneType SceneType { get; }

        public MessageHandlerAttribute(SceneType sceneType)
        {
            this.SceneType = sceneType;
        }
    }
}
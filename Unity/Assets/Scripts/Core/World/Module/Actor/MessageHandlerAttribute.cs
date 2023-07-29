using System;

namespace ET
{
    public class MessageHandlerAttribute: BaseAttribute
    {
        public SceneType SceneType { get; }

        public MessageHandlerAttribute(SceneType sceneType)
        {
            this.SceneType = sceneType;
        }
    }
}
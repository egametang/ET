using System;

namespace ET
{
    public class MessageSessionHandlerAttribute: BaseAttribute
    {
        public SceneType SceneType { get; }

        public MessageSessionHandlerAttribute(SceneType sceneType)
        {
            this.SceneType = sceneType;
        }
    }
}
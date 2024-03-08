using System;

namespace ET
{
    public class MessageSessionHandlerAttribute: BaseAttribute
    {
        public int SceneType { get; }

        public MessageSessionHandlerAttribute(int sceneType)
        {
            this.SceneType = sceneType;
        }
    }
}
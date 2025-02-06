using System;

namespace ET
{
    public class MessageHandlerAttribute: BaseAttribute
    {
        public int SceneType { get; }

        public MessageHandlerAttribute(int sceneType)
        {
            this.SceneType = sceneType;
        }
    }
}
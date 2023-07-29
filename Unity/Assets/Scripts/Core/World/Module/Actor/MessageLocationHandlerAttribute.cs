using System;

namespace ET
{
    public class MessageLocationHandlerAttribute: MessageHandlerAttribute
    {
        public MessageLocationHandlerAttribute(SceneType sceneType): base(sceneType)
        {
        }
    }
}
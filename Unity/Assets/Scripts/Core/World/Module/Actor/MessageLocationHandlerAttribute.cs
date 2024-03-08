using System;

namespace ET
{
    public class MessageLocationHandlerAttribute: MessageHandlerAttribute
    {
        public MessageLocationHandlerAttribute(int sceneType): base(sceneType)
        {
        }
    }
}
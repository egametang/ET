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
    
    public class MessageRpcHandlerAttribute: MessageHandlerAttribute
    {
        public MessageRpcHandlerAttribute(SceneType sceneType): base(sceneType)
        {
        }
    }
}
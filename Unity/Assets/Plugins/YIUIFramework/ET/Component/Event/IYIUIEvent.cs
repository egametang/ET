using System;

namespace ET.Client
{
    public interface IYIUICommonEvent
    {
        /// <summary>
        /// 触发消息
        /// </summary>
        /// <param name="uiScene">当前UI所在的场景</param>
        /// <param name="message">你所监听的消息的数据</param>
        /// <returns></returns>
        ETTask Run(Entity uiScene, object message);
    }
    
    public abstract class YIUICommonEventSystem<T> : IYIUICommonEvent where T: struct
    {
        public async ETTask Run(Entity uiScene, object message)
        {
            await this.Event((Scene)uiScene,(T)message);
        }
        
        protected abstract ETTask Event(Scene uiScene, T message);
    }
    
}
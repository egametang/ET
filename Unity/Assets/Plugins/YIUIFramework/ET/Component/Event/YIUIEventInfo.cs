using System;

namespace ET.Client
{
    //消息分发信息
    public class YIUIEventInfo
    {
        public Type             EventType     { get; } //消息类型
        public string           ComponentName { get; } //UI组件类型 你要具体监听的那个组件是谁
        public IYIUICommonEvent UIEvent       { get; } //反射创建的消息处理实例

        public YIUIEventInfo(Type eventType, string componentName, IYIUICommonEvent uiEvent)
        {
            EventType     = eventType;
            ComponentName = componentName;
            UIEvent       = uiEvent;
        }
    }
}
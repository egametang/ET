using System;

namespace ET.Client
{
    //UI上的消息
    public class YIUIEventAttribut: BaseAttribute
    {
        public Type EventType     { get; } //消息类型
        public Type ComponentType { get; } //UI组件类型 你要具体监听的那个组件是谁

        public YIUIEventAttribut(Type eventType, Type componentType)
        {
            EventType     = eventType;
            ComponentType = componentType;
        }
    }

    
    /*//案例:
    //1:监听某个panel打开前的消息  2:这个panel是XXPanel
    [YIUIEventAttribut(typeof(YIUIEventPanelOpenBefore),typeof(XXPanel))]
    public class YIUIEventPanelOpenLoginPanelBefore : YIUICommonEventSystem<YIUIEventPanelOpenBefore>
    {
        protected override async ETTask Event(Scene uiScene, YIUIEventPanelOpenBefore message)
        {
            await ETTask.CompletedTask;
            Log.Error($" XXPanel 打开前");
        }
    }*/
    
}
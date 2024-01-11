using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 关闭事件 如果没有则算成功
    /// 代表UI正在关闭中  在关闭前来的消息 可以根据实际需求使用
    /// 因为是异步的 所以在过程中注意屏蔽玩家操作之类的
    /// 防止玩家的操作打断你的异步关闭准备工作
    /// 大概率不需要使用这个
    /// UI被关闭
    /// 与OnDisable 不同  Disable 只是显影操作不代表被关闭
    /// 与OnDestroy 不同  Destroy 是摧毁 但是因为有缓存界面的原因 当被缓存时 OnDestroy是不会来的
    /// 这个时候你想要知道是不是被关闭了就必须通过OnClose
    /// baseView除外 因为view的关闭就是隐藏 所以 view的 OnDisable = OnClose
    /// </summary>
    public static partial class YIUIEventSystem
    {
        //返回结果
        //true = 界面可以被关闭
        //false = 界面不允许关闭 需要自行处理各种突发情况
        //这个事件建议没有特殊情况不要用
        public static async ETTask<bool> Close(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return true;
            }

            List<object> iYIUICloseSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUICloseSystem));
            if (iYIUICloseSystems == null)
            {
                return true;
            }

            foreach (IYIUICloseSystem aYIUICloseSystem in iYIUICloseSystems)
            {
                if (aYIUICloseSystem == null)
                {
                    continue;
                }

                try
                {
                    return await aYIUICloseSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            return true;
        }
    }
}
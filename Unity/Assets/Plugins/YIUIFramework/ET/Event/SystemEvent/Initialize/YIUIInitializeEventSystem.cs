using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 初始化事件 与Awake不同 awake是没有初始化UI信息的
    /// 在UI体系中ET的awake 你只能当做构造器的调用 只是知道被new了而已
    /// 你要知道是不是序列化完成了要使用这个事件
    /// </summary>
    public static partial class YIUIEventSystem
    {
        public static void Initialize(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            List<object> iYIUIInitializeSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUIInitializeSystem));
            if (iYIUIInitializeSystems == null)
            {
                return;
            }

            foreach (IYIUIInitializeSystem aYIUIInitializeSystem in iYIUIInitializeSystems)
            {
                if (aYIUIInitializeSystem == null)
                {
                    continue;
                }

                try
                {
                    aYIUIInitializeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
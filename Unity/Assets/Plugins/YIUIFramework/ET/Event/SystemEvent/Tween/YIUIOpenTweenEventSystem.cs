using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 打开动画消息
    /// </summary>
    public static partial class YIUIEventSystem
    {
        public static async ETTask<bool> OpenTween(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return true;
            }

            List<object> iYIUIOpenTweenSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUIOpenTweenSystem));
            if (iYIUIOpenTweenSystems == null)
            {
                return false; //没有则执行默认
            }

            foreach (IYIUIOpenTweenSystem aYIUIOpenTweenSystem in iYIUIOpenTweenSystems)
            {
                if (aYIUIOpenTweenSystem == null)
                {
                    continue;
                }

                try
                {
                    await aYIUIOpenTweenSystem.Run(component);
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
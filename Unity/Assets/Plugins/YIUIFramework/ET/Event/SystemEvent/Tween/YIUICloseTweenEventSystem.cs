using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 关闭动画消息
    /// </summary>
    public static partial class YIUIEventSystem
    {
        public static async ETTask<bool> CloseTween(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return true;
            }

            List<object> iYIUICloseTweenSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUICloseTweenSystem));
            if (iYIUICloseTweenSystems == null)
            {
                return false; //没有则执行默认
            }

            foreach (IYIUICloseTweenSystem aYIUICloseTweenSystem in iYIUICloseTweenSystems)
            {
                if (aYIUICloseTweenSystem == null)
                {
                    continue;
                }

                try
                {
                    await aYIUICloseTweenSystem.Run(component);
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
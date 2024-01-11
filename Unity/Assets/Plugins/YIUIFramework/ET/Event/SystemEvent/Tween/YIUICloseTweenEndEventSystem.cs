using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 关闭动画结束消息
    /// </summary>
    public static partial class YIUIEventSystem
    {
        public static void CloseTweenEnd(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            List<object> iYIUICloseTweenEndSystems =
                    EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUICloseTweenEndSystem));
            if (iYIUICloseTweenEndSystems == null)
            {
                return;
            }

            foreach (IYIUICloseTweenEndSystem aYIUICloseTweenEndSystem in iYIUICloseTweenEndSystems)
            {
                if (aYIUICloseTweenEndSystem == null)
                {
                    continue;
                }

                try
                {
                    aYIUICloseTweenEndSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            return;
        }
    }
}
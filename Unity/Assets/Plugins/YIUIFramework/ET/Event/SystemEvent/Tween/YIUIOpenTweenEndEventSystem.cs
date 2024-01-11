using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 打开动画结束消息
    /// </summary>
    public static partial class YIUIEventSystem
    {
        public static void OpenTweenEnd(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            List<object> iYIUIOpenTweenEndSystems =
                    EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUIOpenTweenEndSystem));
            if (iYIUIOpenTweenEndSystems == null)
            {
                return;
            }

            foreach (IYIUIOpenTweenEndSystem aYIUIOpenTweenEndSystem in iYIUIOpenTweenEndSystems)
            {
                if (aYIUIOpenTweenEndSystem == null)
                {
                    continue;
                }

                try
                {
                    aYIUIOpenTweenEndSystem.Run(component);
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
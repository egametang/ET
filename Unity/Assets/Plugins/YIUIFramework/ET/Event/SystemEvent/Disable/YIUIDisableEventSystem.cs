using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 被隐藏时事件
    /// </summary>
    public static partial class YIUIEventSystem
    {
        public static void Disable(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            List<object> iYIUIDisableSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUIDisableSystem));
            if (iYIUIDisableSystems == null)
            {
                return;
            }

            foreach (IYIUIDisableSystem aYIUIDisableSystem in iYIUIDisableSystems)
            {
                if (aYIUIDisableSystem == null)
                {
                    continue;
                }

                try
                {
                    aYIUIDisableSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 激活可用事件
    /// </summary>
    public static partial class YIUIEventSystem
    {
        public static void Enable(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            List<object> iYIUIEnableSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUIEnableSystem));
            if (iYIUIEnableSystems == null)
            {
                return;
            }

            foreach (IYIUIEnableSystem aYIUIEnableSystem in iYIUIEnableSystems)
            {
                if (aYIUIEnableSystem == null)
                {
                    continue;
                }

                try
                {
                    aYIUIEnableSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
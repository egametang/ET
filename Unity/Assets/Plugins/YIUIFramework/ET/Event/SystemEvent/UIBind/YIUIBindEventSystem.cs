using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 绑定事件 也可以理解为UI的初始化前
    /// </summary>
    public static partial class YIUIEventSystem
    {
        public static void Bind(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            List<object> iYIUIBindSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUIBindSystem));
            if (iYIUIBindSystems == null)
            {
                return;
            }

            foreach (IYIUIBindSystem aYIUIBindSystem in iYIUIBindSystems)
            {
                if (aYIUIBindSystem == null)
                {
                    continue;
                }

                try
                {
                    aYIUIBindSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
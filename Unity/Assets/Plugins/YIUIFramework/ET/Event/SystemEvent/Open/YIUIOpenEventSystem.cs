using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 打开事件 如果没有则算成功
    /// UI的可扩展方法 可以没有
    /// 最高支持5个泛型参数
    /// 打开失败则会关闭UI
    /// ET 中的Awake 只是UI被创建 并没有被初始化
    /// 实际逻辑应该写在 Open 之后
    /// Awake 与 Open 有时序区别 与参数区别
    /// </summary>
    public static partial class YIUIEventSystem
    {
        public static async ETTask<bool> Open(Entity component)
        {
            List<object> iOpenSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUIOpenSystem));
            if (iOpenSystems == null)
            {
                return true;
            }

            foreach (IYIUIOpenSystem aOpenSystem in iOpenSystems)
            {
                if (aOpenSystem == null)
                {
                    continue;
                }

                try
                {
                    return await aOpenSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            return true;
        }

        public static async ETTask<bool> Open<P1>(Entity component, P1 p1)
        {
            List<object> iOpenSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUIOpenSystem<P1>));
            if (iOpenSystems == null)
            {
                return true;
            }

            foreach (IYIUIOpenSystem<P1> aOpenSystem in iOpenSystems)
            {
                if (aOpenSystem == null)
                {
                    continue;
                }

                try
                {
                    return await aOpenSystem.Run(component, p1);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            return true;
        }

        public static async ETTask<bool> Open<P1, P2>(Entity component, P1 p1, P2 p2)
        {
            List<object> iOpenSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUIOpenSystem<P1, P2>));
            if (iOpenSystems == null)
            {
                return true;
            }

            foreach (IYIUIOpenSystem<P1, P2> aOpenSystem in iOpenSystems)
            {
                if (aOpenSystem == null)
                {
                    continue;
                }

                try
                {
                    return await aOpenSystem.Run(component, p1, p2);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            return true;
        }

        public static async ETTask<bool> Open<P1, P2, P3>(Entity component, P1 p1, P2 p2, P3 p3)
        {
            List<object> iOpenSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUIOpenSystem<P1, P2, P3>));
            if (iOpenSystems == null)
            {
                return true;
            }

            foreach (IYIUIOpenSystem<P1, P2, P3> aOpenSystem in iOpenSystems)
            {
                if (aOpenSystem == null)
                {
                    continue;
                }

                try
                {
                    return await aOpenSystem.Run(component, p1, p2, p3);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            return true;
        }

        public static async ETTask<bool> Open<P1, P2, P3, P4>(Entity component, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            List<object> iOpenSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUIOpenSystem<P1, P2, P3, P4>));
            if (iOpenSystems == null)
            {
                return true;
            }

            foreach (IYIUIOpenSystem<P1, P2, P3, P4> aOpenSystem in iOpenSystems)
            {
                if (aOpenSystem == null)
                {
                    continue;
                }

                try
                {
                    return await aOpenSystem.Run(component, p1, p2, p3, p4);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            return true;
        }

        public static async ETTask<bool> Open<P1, P2, P3, P4, P5>(Entity component, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            List<object> iOpenSystems =
                    EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IYIUIOpenSystem<P1, P2, P3, P4, P5>));
            if (iOpenSystems == null)
            {
                return true;
            }

            foreach (IYIUIOpenSystem<P1, P2, P3, P4, P5> aOpenSystem in iOpenSystems)
            {
                if (aOpenSystem == null)
                {
                    continue;
                }

                try
                {
                    return await aOpenSystem.Run(component, p1, p2, p3, p4, p5);
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
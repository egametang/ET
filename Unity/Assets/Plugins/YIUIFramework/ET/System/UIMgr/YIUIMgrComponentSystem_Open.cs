using System;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    public static partial class YIUIMgrComponentSystem
    {
        public static async ETTask<T> OpenPanelAsync<T>(this YIUIMgrComponent self)
                where T : Entity, IAwake, IYIUIOpen
        {
            var info = await self.OpenPanelStartAsync(self.GetPanelName<T>(), self);
            if (info == null) return default;

            var success   = false;
            var component = (T)info.OwnerUIEntity;
            await self.OpenPanelBefore(info);

            try
            {
                success = await info.UIPanel.Open();
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            await self.OpenPanelAfter(info, success);

            return success? component : null;
        }

        public static async ETTask<T> OpenPanelParamAsync<T>(this YIUIMgrComponent self, params object[] paramMore)
                where T : Entity, IYIUIOpen<ParamVo>
        {
            var info = await self.OpenPanelStartAsync(self.GetPanelName<T>(), self);
            if (info == null) return default;

            var success   = false;
            var component = (T)info.OwnerUIEntity;
            await self.OpenPanelBefore(info);

            var p = ParamVo.Get(paramMore);

            try
            {
                success = await info.UIPanel.Open(p);
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            await self.OpenPanelAfter(info, success);

            ParamVo.Put(p);

            return success? component : null;
        }

        public static async ETTask<T> OpenPanelAsync<T, P1>(this YIUIMgrComponent self, P1 p1)
                where T : Entity, IYIUIOpen<P1>
        {
            var info = await self.OpenPanelStartAsync(self.GetPanelName<T>(), self);
            if (info == null) return default;

            var success   = false;
            var component = (T)info.OwnerUIEntity;
            await self.OpenPanelBefore(info);

            try
            {
                success = await info.UIPanel.Open(p1);
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            await self.OpenPanelAfter(info, success);

            return success? component : null;
        }

        public static async ETTask<T> OpenPanelAsync<T, P1, P2>(this YIUIMgrComponent self, P1 p1, P2 p2)
                where T : Entity, IYIUIOpen<P1, P2>
        {
            var info = await self.OpenPanelStartAsync(self.GetPanelName<T>(), self);
            if (info == null) return default;

            var success   = false;
            var component = (T)info.OwnerUIEntity;
            await self.OpenPanelBefore(info);

            try
            {
                success = await info.UIPanel.Open(p1, p2);
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            await self.OpenPanelAfter(info, success);

            return success? component : null;
        }

        public static async ETTask<T> OpenPanelAsync<T, P1, P2, P3>(this YIUIMgrComponent self, P1 p1, P2 p2, P3 p3)
                where T : Entity, IYIUIOpen<P1, P2, P3>
        {
            var info = await self.OpenPanelStartAsync(self.GetPanelName<T>(), self);
            if (info == null) return default;

            var success   = false;
            var component = (T)info.OwnerUIEntity;
            await self.OpenPanelBefore(info);

            try
            {
                success = await info.UIPanel.Open(p1, p2, p3);
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            await self.OpenPanelAfter(info, success);

            return success? component : null;
        }

        public static async ETTask<T> OpenPanelAsync<T, P1, P2, P3, P4>(this YIUIMgrComponent self, P1 p1, P2 p2, P3 p3, P4 p4)
                where T : Entity, IYIUIOpen<P1, P2, P3, P4>
        {
            var info = await self.OpenPanelStartAsync(self.GetPanelName<T>(), self);
            if (info == null) return default;

            var success   = false;
            var component = (T)info.OwnerUIEntity;
            await self.OpenPanelBefore(info);

            try
            {
                success = await info.UIPanel.Open(p1, p2, p3, p4);
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            await self.OpenPanelAfter(info, success);

            return success? component : null;
        }

        public static async ETTask<T> OpenPanelAsync<T, P1, P2, P3, P4, P5>(this YIUIMgrComponent self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
                where T : Entity, IYIUIOpen<P1, P2, P3, P4, P5>
        {
            var info = await self.OpenPanelStartAsync(self.GetPanelName<T>(), self);
            if (info == null) return default;

            var success   = false;
            var component = (T)info.OwnerUIEntity;
            await self.OpenPanelBefore(info);

            try
            {
                success = await info.UIPanel.Open(p1, p2, p3, p4, p5);
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            await self.OpenPanelAfter(info, success);

            return success? component : null;
        }
    }
}
using System;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    public static partial class YIUIPanelComponentSystem
    {
        public static void CloseView<T>(this YIUIPanelComponent self, bool tween = true)
                where T : Entity
        {
            self.CloseViewAsync<T>(tween).Coroutine();
        }

        public static void CloseView(this YIUIPanelComponent self, string resName, bool tween = true)
        {
            self.CloseViewAsync(resName, tween).Coroutine();
        }

        public static async ETTask<bool> CloseViewAsync<T>(this YIUIPanelComponent self, bool tween = true)
                where T : Entity
        {
            var (exist, entity) = self.ExistView<T>();
            if (!exist) return true;

            return await CloseViewAsync(entity, tween);
        }

        public static async ETTask<bool> CloseViewAsync(this YIUIPanelComponent self, string resName, bool tween = true)
        {
            var (exist, entity) = self.ExistView(resName);
            if (!exist) return true;

            return await CloseViewAsync(entity, tween);
        }

        private static async ETTask<bool> CloseViewAsync(Entity entity, bool tween)
        {
            var uibase = entity.GetParent<YIUIComponent>();
            if (uibase == null) return false;

            var viewComponent = uibase.GetComponent<YIUIViewComponent>(true);
            if (viewComponent == null) return false;

            await viewComponent.CloseAsync(tween);

            return true;
        }
    }
}
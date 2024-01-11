using System;
using YIUIFramework;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 公共弹窗界面
    /// </summary>
    [FriendOf(typeof (TipsPanelComponent))]
    public static partial class TipsPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this TipsPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Awake(this TipsPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this TipsPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this TipsPanelComponent self, Type viewType, ParamVo vo)
        {
            return await self.OpenTips(viewType, vo);
        }

        [EntitySystem]
        //消息 回收对象
        private static async ETTask YIUIEvent(this TipsPanelComponent self, EventPutTipsView message)
        {
            await self.PutTips(message.View, message.Tween);
            self.CheckRefCount();
        }

        //对象池的实例化过程
        private static ETTask<Entity> OnCreateViewRenderer(this TipsPanelComponent self, Type uiType)
        {
            return YIUIFactory.InstantiateAsync(uiType, self, self.UIBase.OwnerRectTransform);
        }

        //打开Tips对应的View
        private static async ETTask<bool> OpenTips(this TipsPanelComponent self, Type uiType, ParamVo vo)
        {
            if (!self._AllPool.ContainsKey(uiType))
            {
                async ETTask<Entity> Create()
                {
                    return await self.OnCreateViewRenderer(uiType);
                }

                self._AllPool.Add(uiType, new ObjAsyncCache<Entity>(Create));
            }

            var pool = self._AllPool[uiType];
            var view = await pool.Get();
            if (view == null)
            {
                return self._RefCount > 0;
            }

            var uiComponent = view.GetParent<YIUIComponent>();
            if (uiComponent == null)
            {
                Debug.LogError($"{uiType.Name} 实例化的对象非 YIUIComponent");
                return self._RefCount > 0;
            }

            var viewComponent = uiComponent.GetComponent<YIUIViewComponent>();
            if (viewComponent == null)
            {
                Debug.LogError($"{uiType.Name} 实例化的对象非 YIUIViewComponent");
                return self._RefCount > 0;
            }

            self._RefCount += 1;
            uiComponent.OwnerRectTransform.SetAsLastSibling();

            var result = await viewComponent.Open(vo);
            if (!result)
                await self.PutTips(view,false);
            
            return self._RefCount > 0;
        }

        //回收
        private static async ETTask PutTips(this TipsPanelComponent self, Entity view, bool tween = true)
        {
            if (view == null)
            {
                Debug.LogError($"null对象 请检查");
                return;
            }

            var uiType = view.GetType();
            if (!self._AllPool.ContainsKey(uiType))
            {
                Debug.LogError($"没有这个对象池 请检查 {uiType}");
                return;
            }

            var uiComponent = view.GetParent<YIUIComponent>();
            if (uiComponent == null)
            {
                Debug.LogError($"{uiType.Name} 实例化的对象非 YIUIComponent");
                return;
            }

            var viewComponent = uiComponent.GetComponent<YIUIViewComponent>();
            if (viewComponent == null)
            {
                Debug.LogError($"{uiType.Name} 实例化的对象非 YIUIViewComponent");
                return;
            }

            await viewComponent.CloseAsync(tween);

            var pool = self._AllPool[uiType];
            pool.Put(view);
            self._RefCount -= 1;
        }

        //检查引用计数 如果<=0 就自动关闭UI
        private static void CheckRefCount(this TipsPanelComponent self)
        {
            if (self._RefCount > 0) return;

            self._RefCount = 0;
            self.UIPanel.Close();
        }

        #region YIUIEvent开始

        #endregion YIUIEvent结束
    }
}
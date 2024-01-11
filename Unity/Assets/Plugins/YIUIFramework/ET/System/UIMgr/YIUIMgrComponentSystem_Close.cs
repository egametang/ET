using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    public static partial class YIUIMgrComponentSystem
    {
        /// <summary>
        /// 关闭一个窗口
        /// </summary>
        /// <param name="panelName">名称</param>
        /// <param name="tween">是否调用关闭动画</param>
        /// <param name="ignoreElse">忽略堆栈操作 -- 不要轻易忽略除非你明白 </param>
        public static async ETTask<bool> ClosePanelAsync(this YIUIMgrComponent self, string panelName, bool tween = true, bool ignoreElse = false)
        {
            #if YIUIMACRO_PANEL_OPENCLOSE
            Debug.Log($"<color=yellow> 关闭UI: {panelName} </color>");
            #endif

            self.m_PanelCfgMap.TryGetValue(panelName, out var info);

            if (info?.UIBase == null) return true; //没有也算成功关闭


            EventSystem.Instance.Publish(self.Root(), new YIUIEventPanelCloseBefore
            {
                UIPkgName = info.PkgName, UIResName = info.ResName, UIComponentName = info.Name, PanelLayer = info.PanelLayer,
            });

            if (info.UIPanel.PanelOption.HasFlag(EPanelOption.DisClose))
            {
                var allowClose = false; //是否允许关闭

                //如果继承禁止关闭接口 可返回是否允许关闭自行处理
                if (info.OwnerUIEntity is IYIUIBanClose disClose)
                {
                    allowClose = disClose.DoBanClose();
                }

                if (!allowClose)
                {
                    Debug.LogError($"{panelName} 这个界面禁止被关闭 请检查");
                    return false;
                }
            }

            var success = await YIUIEventSystem.Close(info.OwnerUIEntity);
            if (!success)
            {
                #if YIUIMACRO_PANEL_OPENCLOSE
                Debug.Log($"<color=yellow> 关闭事件返回不允许关闭UI: {panelName} </color>");
                #endif
                return false;
            }

            await info.UIWindow.InternalOnWindowCloseTween(tween);

            if (!ignoreElse)
                await self.RemoveUIAddElse(info);

            self.RemoveUI(info);

            return true;
        }

        public static void ClosePanel(this YIUIMgrComponent self, string panelName, bool tween = true, bool ignoreElse = false)
        {
            self.ClosePanelAsync(panelName, tween, ignoreElse).Coroutine();
        }

        /// <summary>
        /// 关闭一个窗口
        /// 异步等待关闭动画
        /// </summary>
        public static async ETTask<bool> ClosePanelAsync<T>(this YIUIMgrComponent self, bool tween = true, bool ignoreElse = false) where T : Entity
        {
            return await self.ClosePanelAsync(self.GetPanelName<T>(), tween, ignoreElse);
        }

        /// <summary>
        /// 同步关闭窗口
        /// 无法等待关闭动画
        /// </summary>
        public static void ClosePanel<T>(this YIUIMgrComponent self, bool tween = true, bool ignoreElse = false) where T : Entity
        {
            self.ClosePanelAsync(self.GetPanelName<T>(), tween, ignoreElse).Coroutine();
        }

        /// <summary>
        /// 回到指定的界面 其他界面全部关闭
        /// </summary>
        /// <param name="homeName">需要被打开的界面 且这个UI是存在的 否则无法打开</param>
        /// <param name="tween">动画</param>
        public static async ETTask<bool> HomePanel(this YIUIMgrComponent self, string homeName, bool tween = true)
        {
            #if YIUIMACRO_PANEL_OPENCLOSE
            Debug.Log($"<color=yellow> Home关闭其他所有Panel UI: {homeName} </color>");
            #endif

            self.m_PanelCfgMap.TryGetValue(homeName, out var homeInfo);
            if (homeInfo?.UIBase != null)
            {
                return await self.RemoveUIToHome(homeInfo, tween);
            }

            return false;
        }

        public static async ETTask HomePanel<T>(this YIUIMgrComponent self, bool tween = true) where T : Entity
        {
            await self.HomePanel(typeof (T).Name, tween);
        }
    }
}
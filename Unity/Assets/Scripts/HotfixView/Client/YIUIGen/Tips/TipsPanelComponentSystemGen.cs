using System;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [FriendOf(typeof(YIUIComponent))]
    [FriendOf(typeof(YIUIWindowComponent))]
    [FriendOf(typeof(YIUIPanelComponent))]
    [EntitySystemOf(typeof(TipsPanelComponent))]
    public static partial class TipsPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIBind(this TipsPanelComponent self)
        {
            self.UIBind();
        }
        
        private static void UIBind(this TipsPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIComponent>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.BanTween|EWindowOption.BanAwaitOpenTween|EWindowOption.BanAwaitCloseTween|EWindowOption.SkipOtherOpenTween|EWindowOption.SkipOtherCloseTween|EWindowOption.SkipHomeOpenTween|EWindowOption.AllowOptionByTween;
            self.UIPanel.Layer = EPanelLayer.Tips;
            self.UIPanel.PanelOption = EPanelOption.TimeCache;
            self.UIPanel.StackOption = EPanelStackOption.None;
            self.UIPanel.Priority = 9999;
            self.UIPanel.CachePanelTime = 10;


        }
    }
}
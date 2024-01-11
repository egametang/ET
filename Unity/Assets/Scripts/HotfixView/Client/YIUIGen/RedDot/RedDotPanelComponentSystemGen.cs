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
    [EntitySystemOf(typeof(RedDotPanelComponent))]
    public static partial class RedDotPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIBind(this RedDotPanelComponent self)
        {
            self.UIBind();
        }
        
        private static void UIBind(this RedDotPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIComponent>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.BanTween|EWindowOption.BanAwaitOpenTween|EWindowOption.BanAwaitCloseTween|EWindowOption.SkipOtherOpenTween|EWindowOption.SkipOtherCloseTween;
            self.UIPanel.Layer = EPanelLayer.Tips;
            self.UIPanel.PanelOption = EPanelOption.TimeCache;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 1000;
            self.UIPanel.CachePanelTime = 10;

            self.u_ComSearchScroll = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.LoopVerticalScrollRect>("u_ComSearchScroll");
            self.u_ComDropdownSearch = self.UIBase.ComponentTable.FindComponent<TMPro.TMP_Dropdown>("u_ComDropdownSearch");
            self.u_ComStackScroll = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.LoopVerticalScrollRect>("u_ComStackScroll");
            self.u_ComInputChangeCount = self.UIBase.ComponentTable.FindComponent<TMPro.TMP_InputField>("u_ComInputChangeCount");
            self.u_DataDropdownSearch = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueBool>("u_DataDropdownSearch");
            self.u_DataInfoName = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataInfoName");
            self.u_DataToggleUnityEngine = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueBool>("u_DataToggleUnityEngine");
            self.u_DataToggleYIUIBind = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueBool>("u_DataToggleYIUIBind");
            self.u_DataToggleYIUIFramework = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueBool>("u_DataToggleYIUIFramework");
            self.u_DataToggleShowIndex = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueBool>("u_DataToggleShowIndex");
            self.u_DataToggleShowFileName = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueBool>("u_DataToggleShowFileName");
            self.u_DataToggleShowFilePath = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueBool>("u_DataToggleShowFilePath");
            self.u_EventChangeCount = self.UIBase.EventTable.FindEvent<UIEventP1<string>>("u_EventChangeCount");
            self.u_EventChangeCountHandle = self.u_EventChangeCount.Add(self.OnEventChangeCountAction);
            self.u_EventChangeToggleShowFileName = self.UIBase.EventTable.FindEvent<UIEventP1<bool>>("u_EventChangeToggleShowFileName");
            self.u_EventChangeToggleShowFileNameHandle = self.u_EventChangeToggleShowFileName.Add(self.OnEventChangeToggleShowFileNameAction);
            self.u_EventChangeToggleShowFilePath = self.UIBase.EventTable.FindEvent<UIEventP1<bool>>("u_EventChangeToggleShowFilePath");
            self.u_EventChangeToggleShowFilePathHandle = self.u_EventChangeToggleShowFilePath.Add(self.OnEventChangeToggleShowFilePathAction);
            self.u_EventChangeToggleShowStackIndex = self.UIBase.EventTable.FindEvent<UIEventP1<bool>>("u_EventChangeToggleShowStackIndex");
            self.u_EventChangeToggleShowStackIndexHandle = self.u_EventChangeToggleShowStackIndex.Add(self.OnEventChangeToggleShowStackIndexAction);
            self.u_EventChangeToggleUnityEngine = self.UIBase.EventTable.FindEvent<UIEventP1<bool>>("u_EventChangeToggleUnityEngine");
            self.u_EventChangeToggleUnityEngineHandle = self.u_EventChangeToggleUnityEngine.Add(self.OnEventChangeToggleUnityEngineAction);
            self.u_EventChangeToggleYIUIBind = self.UIBase.EventTable.FindEvent<UIEventP1<bool>>("u_EventChangeToggleYIUIBind");
            self.u_EventChangeToggleYIUIBindHandle = self.u_EventChangeToggleYIUIBind.Add(self.OnEventChangeToggleYIUIBindAction);
            self.u_EventChangeToggleYIUIFramework = self.UIBase.EventTable.FindEvent<UIEventP1<bool>>("u_EventChangeToggleYIUIFramework");
            self.u_EventChangeToggleYIUIFrameworkHandle = self.u_EventChangeToggleYIUIFramework.Add(self.OnEventChangeToggleYIUIFrameworkAction);
            self.u_EventClose = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventClose");
            self.u_EventCloseHandle = self.u_EventClose.Add(self.OnEventCloseAction);
            self.u_EventDropdownSearch = self.UIBase.EventTable.FindEvent<UIEventP1<int>>("u_EventDropdownSearch");
            self.u_EventDropdownSearchHandle = self.u_EventDropdownSearch.Add(self.OnEventDropdownSearchAction);
            self.u_EventInputSearchEnd = self.UIBase.EventTable.FindEvent<UIEventP1<string>>("u_EventInputSearchEnd");
            self.u_EventInputSearchEndHandle = self.u_EventInputSearchEnd.Add(self.OnEventInputSearchEndAction);

        }
    }
}
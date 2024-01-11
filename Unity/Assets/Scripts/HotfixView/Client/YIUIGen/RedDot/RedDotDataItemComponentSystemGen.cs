using System;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [FriendOf(typeof(YIUIComponent))]
    [EntitySystemOf(typeof(RedDotDataItemComponent))]
    public static partial class RedDotDataItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIBind(this RedDotDataItemComponent self)
        {
            self.UIBind();
        }
        
        private static void UIBind(this RedDotDataItemComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIComponent>();

            self.u_DataKeyId = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueInt>("u_DataKeyId");
            self.u_DataCount = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueInt>("u_DataCount");
            self.u_DataName = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataName");
            self.u_DataTips = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueBool>("u_DataTips");
            self.u_DataParentCount = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueInt>("u_DataParentCount");
            self.u_DataChildCount = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueInt>("u_DataChildCount");
            self.u_DataShowType = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueBool>("u_DataShowType");
            self.u_DataSwitchTips = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueBool>("u_DataSwitchTips");
            self.u_EventChild = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventChild");
            self.u_EventChildHandle = self.u_EventChild.Add(self.OnEventChildAction);
            self.u_EventClickItem = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventClickItem");
            self.u_EventClickItemHandle = self.u_EventClickItem.Add(self.OnEventClickItemAction);
            self.u_EventParent = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventParent");
            self.u_EventParentHandle = self.u_EventParent.Add(self.OnEventParentAction);
            self.u_EventTips = self.UIBase.EventTable.FindEvent<UIEventP1<bool>>("u_EventTips");
            self.u_EventTipsHandle = self.u_EventTips.Add(self.OnEventTipsAction);

        }
    }
}
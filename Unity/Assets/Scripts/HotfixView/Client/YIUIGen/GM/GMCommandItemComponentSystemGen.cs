using System;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [FriendOf(typeof(YIUIComponent))]
    [EntitySystemOf(typeof(GMCommandItemComponent))]
    public static partial class GMCommandItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIBind(this GMCommandItemComponent self)
        {
            self.UIBind();
        }
        
        private static void UIBind(this GMCommandItemComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIComponent>();

            self.u_ComParamLoop = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.LoopHorizontalScrollRect>("u_ComParamLoop");
            self.u_DataName = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataName");
            self.u_DataShowParamLoop = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueBool>("u_DataShowParamLoop");
            self.u_DataDesc = self.UIBase.DataTable.FindDataValue<YIUIFramework.UIDataValueString>("u_DataDesc");
            self.u_EventRun = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventRun");
            self.u_EventRunHandle = self.u_EventRun.Add(self.OnEventRunAction);

        }
    }
}